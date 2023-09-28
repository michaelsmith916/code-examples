using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace CodeExamples.Lib {
    /**
     * This class byte encodes/decodes the mag data that is retrieved through read and write operations on the 
     * Fargo printer.  This class was pulled from the Fargo Devkit demo code.
     */
    public class DecodeMag {
        /// <summary>
        /// Encoding formats of mag track data
        /// </summary>
        public enum EncodingFormatType {
            /// <summary>
            /// No format - i.e. do not decode, decoded string is just empty
            /// </summary>
            DO_NOT_DECODE,

            /// <summary>
            /// ISO Track 1
            /// </summary>
            DEC_SIX_BIT_PLUS_PARITY,

            /// <summary>
            /// ISO Track 2 and 3
            /// </summary>
            ABA_FOUR_BIT_PLUS_PARITY,
        }


        /// <summary>
        /// Parity enumeration
        /// </summary>
        public enum ParityType {
            /// <summary>
            /// No parity bit needed
            /// </summary>
            NO_PARITY,

            /// <summary>
            /// Even parity bit (number of set bits must be even)
            /// </summary>
            EVEN_PARITY,

            /// <summary>
            /// Odd parity (number of set bits must be odd)
            /// </summary>
            ODD_PARITY,
        }






        /// <summary>
        /// Binary coded decimal table
        /// </summary>
        DataTable bcdFourBitTable;

        /// <summary>
        /// American Banking Association (ABA)
        /// </summary>
        DataTable abaFourBitTable;

        /// <summary>
        /// Dec Six-bit with 1 parity bit.
        /// </summary>
        DataTable decSixBitTable;



        /// <summary>
        /// Create the encoding conversion table
        /// </summary>
        /// <returns></returns>
        private DataTable CreateDataTable() {
            // Create the new data table object
            DataTable myDataTable = new DataTable();

            DataColumn myDataColumn;

            // Create the bit pattern field of the table
            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "binaryData";
            myDataTable.Columns.Add(myDataColumn);

            // Create the ASCII character field of the table
            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "asciiData";
            myDataTable.Columns.Add(myDataColumn);

            return myDataTable;
        }


        /// <summary>
        /// Add the conversion entry to the given conversion table.
        /// </summary>
        /// <param name="myTable">Table to add the row to</param>
        /// <param name="dataIn">Bit pattern field to add</param>
        /// <param name="asciiOut">ASCII character field to add</param>
        private void AddDataToTable(DataTable myTable, string dataIn, string asciiOut) {
            DataRow row;

            // Create the new row in the table
            row = myTable.NewRow();

            // Associate the given fields with the given strings.
            row["binaryData"] = dataIn;
            row["asciiData"] = asciiOut;

            // Add the row to the table object
            myTable.Rows.Add(row);
        }


        /// <summary>
        /// Get the ASCII character associated with the given bit pattern from the given decode table.
        /// </summary>
        /// <param name="myTable">Which table to look up in</param>
        /// <param name="dataIn">Bit pattern to find</param>
        /// <returns>The ASCII character associated with the given bit pattern</returns>
        private string GetAsciiDataFromTable(DataTable myTable, string dataIn) {
            string strExpr = string.Format("binaryData = '{0}'", dataIn);
            DataRow[] foundRow;

            // Find all of the rows that have the given bit pattern (should be only one row)
            foundRow = myTable.Select(strExpr);

            // Use the first row that matched the given bit pattern.
            if (foundRow.GetLength(0) != 0)
                return ((string)foundRow[0]["asciiData"]);
            else
                return ((string)null);
        }


        /// <summary>
        /// Get the bit pattern representation of the given character from the given decode table
        /// </summary>
        /// <param name="myTable">Decode table to use</param>
        /// <param name="dataIn">ASCII character to find within the table </param>
        /// <returns>String of binary digits associated with the given character.</returns>
        private string GetBinaryDataFromTable(DataTable myTable, string dataIn) {
            string strExpr = string.Format("asciiData = '{0}'", dataIn);
            DataRow[] foundRow;

            // Find the rows that match the given ASCII character.
            foundRow = myTable.Select(strExpr);

            // Return the bit pattern of the row
            if (foundRow.GetLength(0) != 0)
                return ((string)foundRow[0]["binaryData"]);
            else
                return ((string)null);
        }


        /// <summary>
        /// Constructor for the decoding object.
        /// </summary>
        public DecodeMag() {
            // Create the conversion data table for the four bit encoding
            //
            // This is used for the BCD conversion
            bcdFourBitTable = CreateDataTable();
            AddDataToTable(bcdFourBitTable, "0000", "0");
            AddDataToTable(bcdFourBitTable, "0001", "1");
            AddDataToTable(bcdFourBitTable, "0010", "2");
            AddDataToTable(bcdFourBitTable, "0011", "3");
            AddDataToTable(bcdFourBitTable, "0100", "4");
            AddDataToTable(bcdFourBitTable, "0101", "5");
            AddDataToTable(bcdFourBitTable, "0110", "6");
            AddDataToTable(bcdFourBitTable, "0111", "7");
            AddDataToTable(bcdFourBitTable, "1000", "8");
            AddDataToTable(bcdFourBitTable, "1001", "9");
            AddDataToTable(bcdFourBitTable, "1010", "A");
            AddDataToTable(bcdFourBitTable, "1011", "B");
            AddDataToTable(bcdFourBitTable, "1100", "C");
            AddDataToTable(bcdFourBitTable, "1101", "D");
            AddDataToTable(bcdFourBitTable, "1110", "E");
            AddDataToTable(bcdFourBitTable, "1111", "F");



            // Create the conversion table for the five bit encoding (actually four bits plus one parity)
            // Note that the parity bit is assumed to be off in this table.
            //
            // Card data on Track 2 & 3 consists of four binary bits and 
            // an odd parity bit for each character. A method for 
            // converting ASCII characters to four-bit Card Data (again 
            // the parity bit is not included in the calculation) is to 
            // subtract 30h from the equivalent ASCII character. For 
            // example, the ASCII character that represents the 
            // number 7 is 37h. Subtract 30h from 37h and the result is 
            // 07h, which represents the four-bit portion of the card 
            // data code for the number 7. An odd parity bit must be 
            // added to the four-bit portion of the character to complete 
            // the Card Data code. A method for converting card data 
            // to ASCII characters is to remove the parity bit from the 
            // Card Data code, then add 30h to the remaining four-bit 
            // portion of the character. The result will be the 0 parity 
            // ASCII character.
            abaFourBitTable = CreateDataTable();

            AddDataToTable(abaFourBitTable, "00000", "0");
            AddDataToTable(abaFourBitTable, "00001", "1");
            AddDataToTable(abaFourBitTable, "00010", "2");
            AddDataToTable(abaFourBitTable, "00011", "3");
            AddDataToTable(abaFourBitTable, "00100", "4");
            AddDataToTable(abaFourBitTable, "00101", "5");
            AddDataToTable(abaFourBitTable, "00110", "6");
            AddDataToTable(abaFourBitTable, "00111", "7");
            AddDataToTable(abaFourBitTable, "01000", "8");
            AddDataToTable(abaFourBitTable, "01001", "9");
            AddDataToTable(abaFourBitTable, "01010", ":");
            AddDataToTable(abaFourBitTable, "01011", ";");
            AddDataToTable(abaFourBitTable, "01100", "<");
            AddDataToTable(abaFourBitTable, "01101", "=");
            AddDataToTable(abaFourBitTable, "01110", ">");
            AddDataToTable(abaFourBitTable, "01111", "?");



            // Create the conversion table for seven bit encoding
            //
            // Really this is six bit plus parity.
            // See: http://nemesis.lonestar.org/reference/telecom/codes/sixbit.html
            // Create the six bit table (used in ISO track 1)
            //
            // Card data on ISO Track 1 consists of six binary bits and an 
            // odd parity bit for each character. A method for 
            // converting ASCII characters to six-bit Card Data (the 
            // parity bit is not included in the calculation) is to subtract 
            // 20h (hex) from the equivalent 0 parity ASCII character 
            // (see Character Conversion Chart). For example, the 
            // ASCII character that represents the percent sign (%) is 
            // 25h. Subtract 20h from 25h and the result is 05h, which 
            // represents the six-bit portion of the card data code for 
            // the percent sign. An odd parity bit must be added to the 
            // six-bit portion of the character to complete the Card Data 
            // code. A method for converting card data to ASCII 
            // characters is to remove the parity bit from the Card Data 
            // code, then add 20h to the remaining six-bit portion of the 
            // character. The result will be the 0 parity ASCII character.             
            decSixBitTable = CreateDataTable();

            // Parity bit is set as off
            AddDataToTable(decSixBitTable, "0000000", " ");
            AddDataToTable(decSixBitTable, "0000001", "!");
            AddDataToTable(decSixBitTable, "0000010", "\\");
            AddDataToTable(decSixBitTable, "0000011", "#");
            AddDataToTable(decSixBitTable, "0000100", "$");
            AddDataToTable(decSixBitTable, "0000101", "%");
            AddDataToTable(decSixBitTable, "0000110", "&");
            AddDataToTable(decSixBitTable, "0000111", "'");

            AddDataToTable(decSixBitTable, "0001000", "(");
            AddDataToTable(decSixBitTable, "0001001", ")");
            AddDataToTable(decSixBitTable, "0001010", "*");
            AddDataToTable(decSixBitTable, "0001011", "+");
            AddDataToTable(decSixBitTable, "0001100", "`");
            AddDataToTable(decSixBitTable, "0001101", "-");
            AddDataToTable(decSixBitTable, "0001110", ".");
            AddDataToTable(decSixBitTable, "0001111", "/");

            AddDataToTable(decSixBitTable, "0010000", "0");
            AddDataToTable(decSixBitTable, "0010001", "1");
            AddDataToTable(decSixBitTable, "0010010", "2");
            AddDataToTable(decSixBitTable, "0010011", "3");
            AddDataToTable(decSixBitTable, "0010100", "4");
            AddDataToTable(decSixBitTable, "0010101", "5");
            AddDataToTable(decSixBitTable, "0010110", "6");
            AddDataToTable(decSixBitTable, "0010111", "7");

            AddDataToTable(decSixBitTable, "0011000", "8");
            AddDataToTable(decSixBitTable, "0011001", "9");
            AddDataToTable(decSixBitTable, "0011010", ":");
            AddDataToTable(decSixBitTable, "0011011", ";");
            AddDataToTable(decSixBitTable, "0011100", "<");
            AddDataToTable(decSixBitTable, "0011101", "=");
            AddDataToTable(decSixBitTable, "0011110", ">");
            AddDataToTable(decSixBitTable, "0011111", "?");

            AddDataToTable(decSixBitTable, "0100000", "@");
            AddDataToTable(decSixBitTable, "0100001", "A");
            AddDataToTable(decSixBitTable, "0100010", "B");
            AddDataToTable(decSixBitTable, "0100011", "C");
            AddDataToTable(decSixBitTable, "0100100", "D");
            AddDataToTable(decSixBitTable, "0100101", "E");
            AddDataToTable(decSixBitTable, "0100110", "F");
            AddDataToTable(decSixBitTable, "0100111", "G");

            AddDataToTable(decSixBitTable, "0101000", "H");
            AddDataToTable(decSixBitTable, "0101001", "I");
            AddDataToTable(decSixBitTable, "0101010", "J");
            AddDataToTable(decSixBitTable, "0101011", "K");
            AddDataToTable(decSixBitTable, "0101100", "L");
            AddDataToTable(decSixBitTable, "0101101", "M");
            AddDataToTable(decSixBitTable, "0101110", "N");
            AddDataToTable(decSixBitTable, "0101111", "O");

            AddDataToTable(decSixBitTable, "0110000", "P");
            AddDataToTable(decSixBitTable, "0110001", "Q");
            AddDataToTable(decSixBitTable, "0110010", "R");
            AddDataToTable(decSixBitTable, "0110011", "S");
            AddDataToTable(decSixBitTable, "0110100", "T");
            AddDataToTable(decSixBitTable, "0110101", "U");
            AddDataToTable(decSixBitTable, "0110110", "V");
            AddDataToTable(decSixBitTable, "0110111", "W");

            AddDataToTable(decSixBitTable, "0111000", "X");
            AddDataToTable(decSixBitTable, "0111001", "Y");
            AddDataToTable(decSixBitTable, "0111010", "Z");
            AddDataToTable(decSixBitTable, "0111011", "[");
            AddDataToTable(decSixBitTable, "0111100", "\\");
            AddDataToTable(decSixBitTable, "0111101", "]");
            AddDataToTable(decSixBitTable, "0111110", "^");
            AddDataToTable(decSixBitTable, "0111111", "_");
        }





        /// <summary>
        /// Decode a string of Mag Data
        /// </summary>
        /// <param name="sData"></param>
        /// <param name="iCharacterSize"></param>
        /// <param name="bParityBits"></param>
        /// <param name="bStartStopSentinel"></param>
        /// <param name="bReverseCharBits">Applicable only for 4 bit</param>
        /// <param name="bReverseString"></param>
        /// <param name="bRemoveLeadingZeros">Applicable only for 8 bit.</param>
        /// <returns></returns>
        public string Decode(string sData, bool bStartStopSentinel, bool bReverseCharBits, bool bReverseString, bool bRemoveLeadingZeros, string sStartSentinel, string sEndSentinel, ParityType parity, EncodingFormatType format) {
            string sReturn = string.Empty;

            switch (format) {
                case EncodingFormatType.ABA_FOUR_BIT_PLUS_PARITY:
                    sReturn = DecodeAbaFourBitPlusParity(sData, bStartStopSentinel, bReverseString, sStartSentinel, sEndSentinel, parity);
                    break;
                case EncodingFormatType.DEC_SIX_BIT_PLUS_PARITY:
                    sReturn = DecodeDecSixBitPlusParity(sData, bStartStopSentinel, bReverseString, sStartSentinel, sEndSentinel, parity);
                    break;
                case EncodingFormatType.DO_NOT_DECODE:
                // Default is to return empty string.
                default:
                    sReturn = string.Empty;
                    break;
            }

            return (sReturn);
        }



        /// <summary>
        /// Compute if a parity bit is needed to be set or not.
        /// </summary>
        /// <param name="inputBits"></param>
        /// <param name="evenParity"></param>
        /// <returns></returns>
        private bool ParityNeeded(string inputBits, ParityType parity) {
            bool result = false;
            int numberSet = 0;
            for (int i = 0; i < inputBits.Length; i++) {
                if (inputBits.Substring(i, 1) == "1") {
                    numberSet++;
                }
            }

            switch (parity) {
                case ParityType.EVEN_PARITY:
                    if ((numberSet & 0x01) == 0x01) {
                        result = true;
                    }
                    break;
                case ParityType.ODD_PARITY:
                    if ((numberSet & 0x01) == 0x00) {
                        result = true;
                    }
                    break;
                default:
                    // result = false;
                    break;
            }

            return (result);
        }






        /// <summary>
        /// Decode the data based on byte length
        /// Assume this is ISO
        /// </summary>
        /// <param name="sData"></param>
        /// <param name="sMask"></param>
        /// <param name="sDecode"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private string DecodeAbaFourBitPlusParity(string sData, bool bStartStopSentinels, bool bReverseString, string sStartSentinel, string sEndSentinel, ParityType parity) {
            int iStartOffset = 0;
            int iEndOffset = 0;
            string sReturn = string.Empty;
            string sWorkingData = string.Empty;
            string sBinaryData = string.Empty;
            string sNibbleBits = string.Empty;
            string sTemp = string.Empty;

            // If no data then nothing to show
            if ((sData == null) || (sData.Length < 4))
                return (null);

            // Swap nibbles
            for (int y = 0; y < sData.Length - 1; y += 2) {
                sWorkingData += sData.Substring(y + 1, 1);
                sWorkingData += sData.Substring(y, 1);
            }

            // Convert the bytes to bits reversing the bit order of each nibble
            for (int y = 0; y < sWorkingData.Length; y++) {
                sTemp = sReverseString(GetBinaryDataFromTable(bcdFourBitTable, sWorkingData.Substring(y, 1)));
                sBinaryData += sTemp;
            }


            // Find the start and stop sentinels if needed
            if (bStartStopSentinels) {
                // Determine what the bit pattern looks like for the sentinels.
                string startAsBits = GetBinaryDataFromTable(abaFourBitTable, sStartSentinel);
                string endAsBits = GetBinaryDataFromTable(abaFourBitTable, sEndSentinel);

                // Adjust the start and end sentinel bit patterns if parity is needed.
                if ((startAsBits != null) && (ParityNeeded(startAsBits, parity))) {
                    startAsBits = "1" + startAsBits.Substring(1);
                }

                if ((endAsBits != null) && (ParityNeeded(endAsBits, parity))) {
                    endAsBits = "1" + endAsBits.Substring(1);
                }

                //string startAsBitsReversed = sReverseString(startAsBits);
                //string endAsBitsReversed = sReverseString(endAsBits);

                // Find the offset of where the start and end sentinels are located.
                if (startAsBits != null) {
                    iStartOffset = iFindStartSentinel(sBinaryData, sReverseString(startAsBits));
                    iStartOffset += 5;
                } else {
                    iStartOffset = 0;
                }

                // Note that the LRC is located after the end sentinel so it also won't be
                // converted into the data returned to the caller.
                if (endAsBits != null) {
                    iEndOffset = iFindStopSentinel(sBinaryData, sReverseString(endAsBits));
                    iEndOffset -= 5;
                } else {
                    iEndOffset = sBinaryData.Length - iStartOffset - 4;
                }

            } else {
                iStartOffset = 0;
                iEndOffset = sBinaryData.Length - iStartOffset - 4;
            }


            // Walk through the bits and convert to ASCII
            for (int y = iStartOffset; y <= iEndOffset; y += 5) {
                // Grab a 5-bit byte and reverse the order of the bits
                sNibbleBits = sReverseString(sBinaryData.Substring(y, 5));

                // Mask off the parity bit for the table
                if (parity != ParityType.NO_PARITY) {
                    // Lop off the most significant bit and replace it with a 0.
                    sNibbleBits = "0" + sNibbleBits.Substring(1);
                }

                sReturn += GetAsciiDataFromTable(abaFourBitTable, sNibbleBits);
            }

            return (sReturn);
        }






        /// <summary>
        /// Decode 7 bit data stream
        /// </summary>
        /// <param name="sData">Incoming data stream (hex string)</param>
        /// <param name="bParityBit">True if parity bit is used in this data stream</param>
        /// <param name="bStartStopSentinels">True if looking for start and stop sentinels</param>
        /// <param name="bReverseByte">True if bytes need to be reversed.</param>
        /// <param name="sStartSentinel">Sentinel used to find start of data</param>
        /// <param name="sEndSentinel">Sentinel used to find end of data</param>
        /// <returns>String decoded</returns>
        private string DecodeDecSixBitPlusParity(string sData, bool bStartStopSentinels, bool bReverseByte, string sStartSentinel, string sEndSentinel, ParityType parity) {
            int iStartOffset = 0;
            int iEndOffset = 0;
            string sReturn = string.Empty;
            string sWorkingData = string.Empty;
            string sBinaryData = string.Empty;
            string sNibbleBits = string.Empty;
            string sTemp = string.Empty;

            // If no data then no output.
            if ((sData == null) || (sData.Length < 4))
                return (null);

            // Swap nibbles
            for (int y = 0; y < sData.Length - 1; y += 2) {
                sWorkingData += sData.Substring(y + 1, 1);
                sWorkingData += sData.Substring(y, 1);
            }

            // Convert the bytes to bits reversing the bit order of each nibble
            for (int y = 0; y < sWorkingData.Length; y++) {
                sTemp = sReverseString(GetBinaryDataFromTable(bcdFourBitTable, sWorkingData.Substring(y, 1)));
                sBinaryData += sTemp;
            }

            // Are the start and stop sentinels included in this stream?
            if (bStartStopSentinels) {
                // Determine what the bit pattern looks like for the sentinels.
                string startAsBits = GetBinaryDataFromTable(decSixBitTable, sStartSentinel);
                string endAsBits = GetBinaryDataFromTable(decSixBitTable, sEndSentinel);

                // Adjust the start and end sentinel bit patterns if parity is needed.
                if ((startAsBits != null) && (ParityNeeded(startAsBits, parity)))
                //if (ParityNeeded(startAsBits, parity))
                {
                    startAsBits = "1" + startAsBits.Substring(1);
                }

                if ((endAsBits != null) && (ParityNeeded(endAsBits, parity)))
                //if (ParityNeeded(endAsBits, parity))
                {
                    endAsBits = "1" + endAsBits.Substring(1);
                }

                //string startAsBitsReversed = sReverseString(startAsBits);
                //string endAsBitsReversed = sReverseString(endAsBits);

                // Find the offset of where the start and end sentinels are.
                if (startAsBits != null) {
                    iStartOffset = iFindStartSentinel(sBinaryData, sReverseString(startAsBits));
                    iStartOffset += 7;
                } else {
                    iStartOffset = 0;
                }

                // Note that the LRC is located after the end sentinel so it also won't be
                // converted into the data returned to the caller.
                if (endAsBits != null) {
                    iEndOffset = iFindStopSentinel(sBinaryData, sReverseString(endAsBits));
                    iEndOffset -= 7;
                } else {
                    iEndOffset = sBinaryData.Length - iStartOffset - 6;
                }

                // Bump the start and ending offset
                //iEndOffset -= 7;
            } else {
                iStartOffset = 0;
                iEndOffset = sBinaryData.Length - iStartOffset - 6;
            }

            // Walk through the characters 7 bits at a time and convert to ASCII
            for (int y = iStartOffset; y <= iEndOffset; y += 7) {
                // Grab a 7-bit byte and reverse the order of the bits
                sNibbleBits = sReverseString(sBinaryData.Substring(y, 7));

                // If parity was used just force the bit off because the
                // table assumes parity bit is always off.
                if (parity != ParityType.NO_PARITY) {
                    // The table assumes all parity bits off...
                    sNibbleBits = "0" + sNibbleBits.Substring(1);
                }

                // Look in the table to find the ASCII character associated with this code.
                sReturn += GetAsciiDataFromTable(decSixBitTable, sNibbleBits);
            }

            return (sReturn);
        }










        /// <summary>
        /// Find the start sentinel within the data stream
        /// </summary>
        /// <param name="sData"></param>
        /// <param name="sStartSentinel"></param>
        /// <returns></returns>
        private int iFindStartSentinel(string sData, string sStartSentinel) {
            // Find the start sentinel within the string by searching from the start of the stream
            return (sData.IndexOf(sStartSentinel));
        }


        /// <summary>
        /// Find the stop sentinel within the data stream
        /// </summary>
        /// <param name="sData"></param>
        /// <param name="sStopSentinel"></param>
        /// <returns></returns>
        private int iFindStopSentinel(string sData, string sStopSentinel) {
            // Find the stop sentinel within the string by searching from the end of the stream.
            return (sData.LastIndexOf(sStopSentinel));
        }




        /// <summary>
        /// Reverses the characters in a string.
        /// </summary>
        /// <param name="sNibble"></param>
        /// <returns></returns>
        private string sReverseString(string sString) {
            string sReturn = string.Empty;

            if (sString.Length < 2)
                return (sString);

            for (int i = sString.Length - 1; i >= 0; i--) {
                sReturn += sString.Substring(i, 1);
            }

            return (sReturn);
        }
    }
}
