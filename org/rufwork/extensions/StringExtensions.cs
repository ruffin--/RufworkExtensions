// =========================== LICENSE ===============================
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
// ======================== EO LICENSE ===============================

//using System.Security.Cryptography;

using org.rufwork.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace org.rufwork.extensions
{
    public static class StringExtensions
    {
        #region Private Methods
        // It's probably the sad tragedy of micro-optimization theater (http://blog.codinghorror.com/the-sad-tragedy-of-micro-optimization-theater/),
        // but I don't want to be checking a bCaseSensitive switch on every comparision, so I'm splitting the
        // engines up.
        // TODO: Insensitive probably needs some optimization. Eg, if anglais, we can do lots
        // of ASCII-ish cheats, right? Any rate, need to consider optimization.
        private static bool _containsOutsideOfQuotesCaseINsensitive(string str, string strToFind, params char[] astrSplittingTokens)
        {
            bool foundIt = false;

            for (int i = 0; i < str.Length; i++)
            {
                while (i < str.Length && !astrSplittingTokens.ContainsCaseInsensitive(str[i]) && !str[i].EqualsCaseInsensitive(strToFind[0]))
                    i++;

                if (i < str.Length)
                {
                    // Should splitters also be case insensitive? I'm going to say yes.
                    if (astrSplittingTokens.ContainsCaseInsensitive(str[i]))
                    {
                        // Remember which of the astrSplittingTokens was found to find the right closing char.
                        char splitterFound = str[i];
                        i++;
                        while (i < str.Length)
                        {
                            if (str[i].EqualsCaseInsensitive(splitterFound))
                                if (i + 1 < str.Length)
                                    if (str[i + 1].EqualsCaseInsensitive(splitterFound))
                                        i = i + 2;
                                    else
                                        break;
                                else
                                    break;  // we're at the end of `str`; it'll kick out in the initial while in the next pass.
                            else
                                i++;
                        }
                    }
                    else
                    {
                        // else this should be equal to the first char in the search string.
                        int foundStart = i;
                        while (i < str.Length && i - foundStart < strToFind.Length && str[i].EqualsCaseInsensitive(strToFind[i - foundStart]))
                            i++;
                        if (i - foundStart == strToFind.Length)
                        {
                            foundIt = true;
                            break;
                        }
                    }
                }
            }

            return foundIt;
        }

        private static bool _containsOutsideOfQuotesCaseSENSITIVE(string str, string strToFind, params char[] astrSplittingTokens)
        {
            bool foundIt = false;

            for (int i = 0; i < str.Length; i++)
            {
                while (i < str.Length && !astrSplittingTokens.Contains(str[i]) && !str[i].Equals(strToFind[0]))
                    i++;

                if (i < str.Length)
                {
                    if (astrSplittingTokens.Contains(str[i]))
                    {
                        // Remember which of the astrSplittingTokens was found to find the right closing char.
                        char splitterFound = str[i];
                        i++;
                        while (i < str.Length)
                        {
                            if (str[i].Equals(splitterFound))
                                if (i + 1 < str.Length)
                                    if (str[i + 1].Equals(splitterFound))
                                        i = i + 2;
                                    else
                                        break;
                                else
                                    break;  // we're at the end of `str`; it'll kick out in the initial while in the next pass.
                            else
                                i++;
                        }
                    }
                    else
                    {
                        // else this should be equal to the first char in the search string.
                        int foundStart = i;
                        while (i < str.Length && i - foundStart < strToFind.Length && str[i].Equals(strToFind[i - foundStart]))
                            i++;
                        if (i - foundStart == strToFind.Length)
                        {
                            foundIt = true;
                            break;
                        }
                    }
                }
            }

            return foundIt;
        }

        private static string _AsteriskizeString(string str, int intLength, bool useStarOnOversize, bool displayEnd)
        {
            str = displayEnd ? str.Substring(str.Length - intLength) : str.Substring(0, intLength);
            if (useStarOnOversize) str = displayEnd
                ? str.ReplaceSingleChar(0, '*')
                : str.ReplaceSingleChar(str.Length - 1, '*');

            return str;
        }

        private static bool _NotEmptyString(String s)
        {
            // TODO: Why not `!string.IsNullOrEmpty(s)`?  Guess we can't get a null from
            // the split in StringToNonWhitespaceTokens and this is faster?
            return !s.Equals("");
        }

        private static string _dbCleanAndQuote(string strIn, bool addQuotes = true)
        {
            strIn = Regex.Replace(strIn, @"\r\n?|\n", "\n");
            strIn = strIn.Replace("'", "''");
            strIn = strIn.Replace("" + (char)8217, "''");
            strIn = strIn.Replace(";", @"\;");
            strIn = addQuotes ? "'" + strIn + "'" : strIn;

            return strIn;
        }
        #endregion Private Methods

        #region Parse subset of string
        /// <summary>
        /// Replacement for string's Substring that won't bork if the start location is past
        /// the original's length (returns string.Empty in that case) or if the length for the
        /// substring is longer than what's left after starting at intStart (returns as much string as
        /// there is after intStart).
        /// </summary>
        /// <param name="self">The string being substringed</param>
        /// <param name="intStart">0-indexed character from which to start the substring.</param>
        /// <param name="intLength">Length of substring to attempt to take.</param>
        /// <returns>Returns the normal substring, string.Empty if start is past the end of the string, or
        /// as much of the string as there is if the length of the substring would pass the end of `self`.</returns>
        public static string SafeSubstring(this string self, int intStart, int intLength)
        {
            if (intStart > self.Length)
                return string.Empty;
            else
                return self.Substring(intStart, Math.Min(intStart + intLength, self.Length - intStart));
        }

        // Using `ToByte` just to illustrate cleanly that we're ANDing on
        // the leading bit.
        // Note that C# 6.0 might allow "real" binary representations:
        // http://roslyn.codeplex.com/wikipage?title=Language%20Feature%20Status
        public static string CutToUTF8Length(this string str, int byteLength)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            string returnValue = string.Empty;

            if (byteArray.Length > byteLength)
            {
                int bytePointer = byteLength;

                // Check high bit to see if we're [potentially] in the middle of a multi-byte char
                if (bytePointer >= 0 && (byteArray[bytePointer] & Convert.ToByte("10000000", 2)) > 0)
                {
                    while (bytePointer >= 0 && Convert.ToByte("11000000", 2) != (byteArray[bytePointer] & Convert.ToByte("11000000", 2)))
                    {
                        bytePointer--;
                    }
                }

                // See if we had 1s in the high bit all the way back. If so, we're toast. Return empty string.
                if (0 != bytePointer)
                {
                    byte[] cutValue = new byte[bytePointer];
                    Array.Copy(byteArray, cutValue, bytePointer);
                    returnValue = Encoding.UTF8.GetString(cutValue, 0, cutValue.Length);
                }
            }
            else
            {
                returnValue = str;
            }

            return returnValue;
        }

        // Note that this is length by bytes.
        public static string SafeUTFSubstring(this string self, int intStartCharacter, int intLengthInBytes)
        {
            if (intStartCharacter > self.Length)
                return string.Empty;
            else
                return self.Substring(intStartCharacter).CutToUTF8Length(Math.Min(intStartCharacter + intLengthInBytes, self.LengthUTF8()));
        }
        #endregion Parse subset of string

        #region String manipulation (string-to-reformatted-string)

        public static string Splice(this string str, string toInsert, int spliceLoc)
        {
            string ret = str;
            if (!string.IsNullOrEmpty(str) && str.Length >= spliceLoc && spliceLoc > -1)
            {
                ret = str.Substring(0, spliceLoc) + toInsert + str.Substring(spliceLoc);
            }
            return ret;
        }

        public static string DraconianWrap(this string str, int lineLength, string lineEnding = "\n")
        {
            StringBuilder stringBuilder = new StringBuilder();
            int stringPos = 0;
            while (stringPos < str.Length)
            {
                int substringLength = stringPos + lineLength > str.Length ? str.Length - stringPos : lineLength;
                string substring = str.Substring(stringPos, substringLength);
                stringBuilder.Append(substring + lineEnding);
                stringPos += substringLength;
            }
            return stringBuilder.ToString();
        }

        // Note that this doesn't work with Hebrew characters with vowels,
        // apparently (though you could argue it kind of does, iiuc)
        // See stackoverflow.com/questions/15029238
        public static string ReverseString(this string str)
        {
            if (null == str)
                return null;

            char[] aReverseMe = str.ToCharArray();
            Array.Reverse(aReverseMe);
            return new string(aReverseMe);
        }

        public static string OperateOnNonQuotedChunks(this string str,
            Func<string, string> chunkProcessor,
            params char[] astrSplittingTokens)
        {
            string strReturn = string.Empty;
            string strUnquotedChunk = string.Empty;

            for (int i = 0; i < str.Length; i++)
            {
                while (i < str.Length && !astrSplittingTokens.Contains(str[i]))
                {
                    strUnquotedChunk += str[i]; // I guess we could use indexOf, but that'd make the `params` more difficult to handle
                    i++;
                }
                strReturn += chunkProcessor(strUnquotedChunk);      // TODO: StringBuilder
                strUnquotedChunk = string.Empty;

                // So we're either at the end of the string or we're in a quote.
                if (i < str.Length)
                {
                    char splitterFound = str[i];
                    i++;
                    while (i < str.Length)
                    {
                        if (str[i].Equals(splitterFound))
                            // If we have two of the same splitter char in a row, we're going to
                            // treat it as an escape sequence.
                            // TODO: Could make that optional.
                            if (i + 1 < str.Length)
                                if (str[i + 1].Equals(splitterFound))
                                    i = i + 2;
                                else
                                    break;
                            else
                                break;  // we're at the end of `str`; it'll kick out in the initial while in the next pass.
                        else
                            i++;
                    }
                }
            }

            return strReturn;
        }

        public static string ReplaceSingleChar(this string str, int intCharIndex, char chr)
        {
            char[] achr = str.ToCharArray();
            achr[intCharIndex] = chr;
            return new string(achr);
        }

        public static string ReplaceCaseInsensitiveFind(this string str, string findMe, string newValue)
        {
            return Regex.Replace(str,
                Regex.Escape(findMe),
                Regex.Replace(newValue, "\\$[0-9]+", @"$$$0"),
                RegexOptions.IgnoreCase);
        }


        public static string ExtractHtmlFromClipboardString(this string str)
        {
            return str.ExtractBetweenFirstInstanceofDelimiters("<!--StartFragment-->", "<!--EndFragment-->");
        }

        public static string ExtractBetweenFirstInstanceofDelimiters(this string str, string delimiterStartAfter, string delimiterEndBefore, bool iLiedUseLast = false)
        {
            string strRun = string.Empty;
            int indexStart = iLiedUseLast ? str.LastIndexOf(delimiterStartAfter) : str.IndexOf(delimiterStartAfter);
            int indexEnd = iLiedUseLast ? str.LastIndexOf(delimiterEndBefore) : str.IndexOf(delimiterEndBefore);

            if (indexStart < indexEnd && indexStart > -1)
            {
                strRun = str.Substring(indexStart + delimiterStartAfter.Length, indexEnd - (indexStart + delimiterStartAfter.Length));
            }
            else
            {
                strRun = string.Empty;  // No luck, Ending not after Start or not found; go back to nothing.
            }

            return strRun;
        }

        /// <summary>
        /// Takes the current string and, if it's over the total length sent in intLength,
        /// truncates to the pad value. If it's under the length in intLength, a coventional
        /// `PadLeft(intLength)` will be performed.
        /// </summary>
        /// <param name="str">The string being padded or limited.</param>
        /// <param name="intLength">The length to which to pad or limit</param>
        /// <param name="useStarOnOversize">If true and str is longer than intLength, str will be truncated
        /// to intLength - 1 and then an asterisk will be added onto the end. So "This is a long line" maxed
        /// at 5 would return "This*".</param>
        /// <param name="displayEnd">There are times it might be useful to display the end of a string rather
        /// than the beginning when all of the string cannot be displayed. If so, set this to true.</param>
        /// <returns>The limited or padded (or full) string.</returns>
        public static string PadLeftWithMax(this string str, int intLength, bool useStarOnOversize = false, bool displayEnd = false)
        {
            if (str.Length > intLength)
                str = _AsteriskizeString(str, intLength, useStarOnOversize, displayEnd);
            else
                str = str.PadLeft(intLength);

            return str;
        }

        /// <summary>
        /// Takes the current string and, if it's over the total length sent in intLength,
        /// truncates to the pad value. If it's under the length in intLength, a coventional
        /// `PadRight(intLength)` will be performed.
        /// </summary>
        /// <param name="str">The string being padded or limited.</param>
        /// <param name="intLength">The length to which to pad or limit</param>
        /// <param name="useStarOnOversize">If true and str is longer than intLength, str will be truncated
        /// to intLength - 1 and then an asterisk will be added onto the end. So "This is a long line" maxed
        /// at 5 would return "This*".</param>
        /// <param name="displayEnd">There are times it might be useful to display the end of a string rather
        /// than the beginning when all of the string cannot be displayed. If so, set this to true.</param>
        /// <returns>The limited or padded (or full) string.</returns>
        public static string PadRightWithMax(this string str, int intLength, bool useStarOnOversize = false, bool displayEnd = false)
        {
            if (str.Length > intLength)
                str = _AsteriskizeString(str, intLength, useStarOnOversize, displayEnd);
            else
                str = str.PadRight(intLength);

            return str;
        }

        public static string FlattenWhitespace(this string strIn)
        {
            return System.Text.RegularExpressions.Regex.Replace(strIn, @"[\s\n]+", " ");
        }

        public static string ScrubValue(this string strToScrub)
        {
            string strReturn = strToScrub;

            strReturn = strReturn.Replace("'", "''");

            return strReturn;
        }

        public static string ToQuotedPrintable(this string self)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(self);
            StringBuilder sbInner = new StringBuilder();
            StringBuilder sbOuter = new StringBuilder();

            foreach (byte byt in bytes)
            {
                int charLenQP = (byt >= 33 && byt <= 126 && byt != 61) ? 1 : 3;
                if (sbInner.Length + charLenQP > 75)
                {
                    sbOuter.Append(sbInner + "=\r\n");
                    sbInner = new StringBuilder();
                }

                if (1 == charLenQP)
                {
                    sbInner.Append((char)byt);
                }
                else
                {
                    sbInner.Append("=" + byt.ToString("X2"));
                }
            }
            sbOuter.Append(sbInner);
            return sbOuter.ToString();
        }

        // Only replace double single quotes inside of single quotes.
        public static string BacktickQuotes(this string strToClean)
        {
            bool inQuotes = false;
            string strOut = string.Empty;
            for (int i = 0; i < strToClean.Length - 1; i++)
            {
                if (strToClean[i].Equals('\'') && strToClean[i + 1].Equals('\''))
                {
                    strOut += inQuotes ? "`" : "''";
                    i++;
                }
                else if (strToClean[i].Equals('\''))
                {
                    strOut += '\'';
                    inQuotes = !inQuotes;
                }
                else
                    strOut += strToClean[i];
            }
            strOut += strToClean[strToClean.Length - 1];
            //Logger.WriteLine(strOut);
            return strOut;
        }

        public static string RemoveLastNChars(this string str, int charsToDelete)
        {
            return str.Remove(str.Length - charsToDelete);
        }

        public static string Slice(this string str, int intSlice)
        {
            string ret = str;

            if (0 == intSlice)
            {
                ret = str;
            }
            else if (intSlice > 0)
            {
                ret = str.Substring(0, intSlice);
            }
            else
            {
                // Remember that intSlice is negative here
                ret = str.Remove(str.Length + intSlice);
            }
            return ret;
        }

        /// <summary>
        /// Cleans a string to insert into SQL so that it shouldn't allow
        /// SQL injection when run (but might). Not guaranteeing it's
        /// particularly robust at this point, I don't think.
        /// Adds single quotes to both ends of the string (start and end).
        /// </summary>
        /// <param name="strIn">The string being cleaned. Should NOT be the entire SQL string, but just what might be pushed into a quoted value.</param>
        /// <returns>The cleaned, now single-quoted SQL string value.</returns>
        public static string DbCleanAndQuote(this string strIn)
        {
            return _dbCleanAndQuote(strIn, true);
        }

        public static string DbCleanNoQuote(this string strIn)
        {
            return _dbCleanAndQuote(strIn, false);
        }
        #endregion String manipulation (string-to-reformatted-string)

        #region string checks (return boolean based on some condition/set of conditions)
        public static bool ContainsWhitespace(this string str)
        {
            Regex regexp = new Regex(@"\s");
            return regexp.IsMatch(str);
        }

        /// <summary>
        /// Looks for a string that isn't within a quoted section of the parent string.
        /// This overload will use default quote character of ' and a
        /// StringComparison type of CurrentCultureIgnorecase.
        /// </summary>
        /// <param name="str">The string being searched; `this`</param>
        /// <param name="strToFind">String that we want to find outside of quotes in the "calling" or parent string.</param>
        /// <returns>True if string is found, false is not.</returns>
        public static bool ContainsOutsideOfQuotes(this string str, string strToFind)
        {
            return str.ContainsOutsideOfQuotes(strToFind, StringComparison.CurrentCultureIgnoreCase, '\'');
        }

        /// <summary>
        /// Looks for a string that isn't within a quoted section of the parent string.
        /// This overload will accepts any number of splitting tokens as trailing params, and uses
        /// StringComparison type of CurrentCultureIgnorecase.
        /// </summary>
        /// <param name="str">The string being searched; `this`</param>
        /// <param name="strToFind">String that we want to find outside of quotes in the "calling" or parent string.</param>
        /// <param name="astrSplittingTokens">The tokens that can be used to declare the start and stop of an
        /// escaped string.</param>
        /// <returns>True if it's there, false if it isn't.</returns>
        public static bool ContainsOutsideOfQuotes(this string str, string strToFind, params char[] astrSplittingTokens)
        {
            return str.ContainsOutsideOfQuotes(strToFind, StringComparison.CurrentCultureIgnoreCase, astrSplittingTokens);
        }

        public static bool ContainsOutsideOfQuotes(this string str, string strToFind, StringComparison stringComparison)
        {
            return str.ContainsOutsideOfQuotes(strToFind, stringComparison, new[] { '\'' });
        }

        /// <summary>
        /// Looks for a string that isn't within a quoted section of the parent string.
        /// If the double-quote is passed in as a "splitting token", and you're looking for "test" within "This is 'a test' isn''t it?", it'd return false, because
        /// "test" is within ' and '.
        ///
        /// Note: I'm not handling stringComparison yet.
        /// </summary>
        /// <param name="strToFind">String that we want to find outside of quotes in the "calling" or parent string.</param>
        /// <param name="stringComparison">Currently ignored.</param>
        /// <param name="astrSplittingTokens">The tokens that can be used to declare the start and stop of an
        /// escaped string.</param>
        /// <returns>True if it's there, false if it isn't.</returns>
        public static bool ContainsOutsideOfQuotes(this string str, string strToFind,
            StringComparison stringComparison,
            params char[] astrSplittingTokens)
        {
            switch (stringComparison)
            {
                case StringComparison.CurrentCultureIgnoreCase:
                //case StringComparison.InvariantCultureIgnoreCase: // not valid in PCL, apparently.
                case StringComparison.OrdinalIgnoreCase:
                    return _containsOutsideOfQuotesCaseINsensitive(str, strToFind, astrSplittingTokens);

                default:
                    return _containsOutsideOfQuotesCaseSENSITIVE(str, strToFind, astrSplittingTokens);
            }
        }

        public static bool IsNumeric(this string str)
        {
            double dblDummy;
            return double.TryParse(str, out dblDummy);
            //return str.All(c => char.IsDigit(c) || c == '.'); // <<< Advantage is no issue with overflows, which might be a problem with double.TryParse.  I'll ignore that for now (I could wrap for an overflow exception and then fallback to this).
        }

        public static bool CouldBeUrl(this string str)
        {
            //return str.StartsWith("http") && str.Contains("://");
            return Uri.IsWellFormedUriString(str, UriKind.Absolute);
        }

        /// <summary>
        /// This method checks if a specific leading string starts immediately before a zero-width
        /// cursor placed at the zero-based index given in `zeroBasedPos` in the string being tested.
        /// "01234".Leads(2, "01") == true
        /// Note that empty test strings will always return true.
        /// </summary>
        /// <param name="str">The larger "parent" string to test</param>
        /// <param name="zeroBasedPos">The location of the zero-width "cursor" demarking the test start.</param>
        /// <param name="lead">The string to match in the parent string</param>
        /// <returns>True if the string leads the given cursor location. False otherwise. "" as a leading
        /// string will always return true.</returns>
        public static bool Leads(this string str, int zeroBasedPos, string lead)
        {
            bool ret = false;

            if (null != lead && zeroBasedPos - lead.Length >= 0)
            {
                ret = true;

                int i = 1;
                while (i <= lead.Length && ret)
                {
                    ret = ret && str[zeroBasedPos - i].Equals(lead[lead.Length - i]);
                    i++;
                }
            }
            return ret;
        }

        /// <summary>
        /// This method checks if a specific trailing string starts immediately after a zero-width
        /// cursor placed at the zero-based index given in `zeroBasedPos` in the string being tested.
        /// "01234".Trails(2, "234") == true
        /// Note that empty test strings will always return true.
        /// </summary>
        /// <param name="str">The larger "parent" string to test</param>
        /// <param name="zeroBasedPos">The location of the zero-width "cursor" demarking the test start.</param>
        /// <param name="trail">The string to match in the parent string</param>
        /// <returns>True if the string trails the given cursor location. False otherwise. "" as a trailing
        /// string will always return true.</returns>
        public static bool Trails(this string str, int zeroBasedPos, string trail)
        {
            bool ret = false;

            if (null != trail && zeroBasedPos + trail.Length <= str.Length)
            {
                ret = true;

                int i = 0;
                while (i < trail.Length && ret)
                {
                    ret = ret && str[zeroBasedPos + i].Equals(trail[i]);
                    i++;
                }
            }
            return ret;
        }
        #endregion string checks (return boolean based on some condition/set of conditions)

        #region Splits/strings into some IEnumerable
        public static string[] Split(this string str, string splitter)
        {
            return str.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static Queue<string> SplitSeeingSingleQuotesAndBackticks(this string strToSplit, string strSplittingToken, bool bIncludeToken, bool bTrimResults = true)
        {
            return strToSplit.SplitSeeingQuotes(strSplittingToken, bIncludeToken, bTrimResults, '\'', '`');
        }

        // Another cheesy regular expression end run.  Don't overcomplicate jive.
        // This should split up strings with multiple commands into, well, multiple commands.
        // Remember the respect tokens within backticks to support MySQL style backtick quotes in
        // statements like CREATE TABLE `DbVersion`...
        public static Queue<string> SplitSeeingQuotes(this string strToSplit, string strSplittingToken, bool bIncludeToken, bool bTrimResults, params char[] validQuoteChars)
        {
            Queue<string> qReturn = new Queue<string>();
            StringBuilder stringBuilder = new StringBuilder();

            // TODO: A smarter way to ensure you're comparing apples to apples
            // in the first byte knockout comparison.
            string STRTOSPLIT = strToSplit.ToUpper();
            string STRSPLITTINGTOKEN = strSplittingToken.ToUpper();

            bool inQuotes = false;
            char chrCurrentSplitter = validQuoteChars[0];   // dummy value.

            for (int i = 0; i < strToSplit.Length; i++)
            {
                // TOOD: Reconsider efficiency of these checks.
                if (!inQuotes && validQuoteChars.Contains(strToSplit[i]))
                {
                    inQuotes = true;
                    chrCurrentSplitter = strToSplit[i];
                    stringBuilder.Append(strToSplit[i]);
                }
                else if (inQuotes && strToSplit[i].Equals(chrCurrentSplitter))
                {
                    inQuotes = false;
                    stringBuilder.Append(strToSplit[i]);
                }
                else if (!inQuotes
                    && STRSPLITTINGTOKEN[0] == STRTOSPLIT[i]
                    && strToSplit.Length >= i + strSplittingToken.Length
                    && strSplittingToken.Equals(strToSplit.Substring(i, strSplittingToken.Length), StringComparison.CurrentCultureIgnoreCase))
                {
                    if (bIncludeToken)
                    {
                        stringBuilder.Append(strSplittingToken);
                    }
                    if (stringBuilder.Length > 0 && (!bTrimResults || stringBuilder.ToString().Trim().Length > 0))
                    {
                        qReturn.Enqueue(stringBuilder.ToString());
                        stringBuilder = new StringBuilder();
                    }
                    i = i + (strSplittingToken.Length - 1); // -1 for the char we've already got in strToSplit[i]
                }
                else
                {
                    stringBuilder.Append(strToSplit[i]);
                }
            }

            if (stringBuilder.Length > 0 && (!bTrimResults || stringBuilder.ToString().Trim().Length > 0))
            {
                qReturn.Enqueue(stringBuilder.ToString());
            }

            return qReturn;
        }

        public static string[] StringToNonWhitespaceTokens(this string strToToke)
        {
            string[] astrAllTokens = strToToke.Split();
            string[] astrCmdTokens =
                Array.FindAll(astrAllTokens, _NotEmptyString);
            return astrCmdTokens;
        }

        /// <summary>
        /// Splits the string into an array of non-whitespace tokens
        /// split by any whitespace. Strips commas and does not return
        /// tokens that trim to empty strings.
        /// </summary>
        /// <param name="strToToke">The string this extension is called upon</param>
        /// <returns>String array of non-whitespace tokens.</returns>
        public static string[] StringToNonWhitespaceTokens2(this string strToToke)
        {
            return Regex.Split(strToToke, @"[\(\)\s,]+").Where(s => s != String.Empty).ToArray<string>(); // TODO: Better way of doing this.  Can I add to regex intelligently?
        }

        // TODO: Consider having a "max lines to return" governor to make sure we don't get memory crazy.
        public static string[] LinesAsArray(this string str, int intWrapLength = -1)
        {
            string[] astrRun = str.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (intWrapLength > 1)
            {
                Queue<string> qRun = new Queue<string>();
                foreach (string strLine in astrRun)
                {
                    foreach (string strWrappedLine in strLine.DraconianWrap(intWrapLength).LinesAsArray())
                    {
                        qRun.Enqueue(strWrappedLine);
                    }
                }
                astrRun = qRun.ToArray();
            }
            return astrRun;
        }

        // Yes, I gave up on RegExp and used a char array.  Sue me.
        // Honestly, this is much more straightforward.  It's like a regexp
        // as an exploded view.
        // Honestly not sure why this isn't in a "SqlSpecificStringExtensions" class. <<< This. +1.
        // It will be soonish.
        public static string[] SqlToTokens(this string strToToke)
        {
            char[] achrSql = strToToke.ToCharArray();
            Queue<string> qString = new Queue<string>();
            string strTemp = "";

            for (int i = 0; i < achrSql.Length; i++)
            {
                switch (achrSql[i])
                {
                    case ' ':
                    case '\r':
                    case '\n':
                    case '\t':
                        if (!string.IsNullOrWhiteSpace(strTemp))
                        {
                            qString.Enqueue(strTemp);
                        }
                        strTemp = "";
                        while (string.IsNullOrWhiteSpace(achrSql[i].ToString()) && i < achrSql.Length)
                        {
                            i++;
                        }
                        if (i < achrSql.Length)
                        {
                            i--;    // pick back up with next non-space character.
                        }
                        break;

                    case '\'':
                        i++;
                        while ('\'' != achrSql[i])
                        {
                            strTemp += achrSql[i++];
                        }
                        qString.Enqueue("'" + strTemp + "'");
                        strTemp = "";
                        break;

                    case '(':
                    case ')':
                        break;

                    case ',':
                        if (string.Empty != strTemp.Trim())
                        {
                            qString.Enqueue(strTemp);
                        }
                        strTemp = "";
                        break;

                    // TODO: Handle functions more cleanly.  Maybe translate to easily parsed, paren-less strings?
                    // This is EMBARRASSINGLY hacky.
                    case 'N':
                        if (i + 4 < achrSql.Length)
                        {
                            if (achrSql[i + 1].Equals('O')
                                && achrSql[i + 2].Equals('W')
                                && achrSql[i + 3].Equals('(')
                                && achrSql[i + 4].Equals(')')
                            )
                            {
                                qString.Enqueue("NOW()");
                                i = i + 4;
                                strTemp = "";
                            }
                            else
                            {
                                goto default;
                            }
                        }
                        break;

                    default:
                        strTemp += achrSql[i];
                        break;
                }
            }
            if (string.Empty != strTemp.Trim())
            {
                qString.Enqueue(strTemp);
            }
            return qString.ToArray();
        }

        // Not strictly an IEnumerable (okay okay, not one at all), but returns two strings
        // based on content, so fitting into Splits/string parses region.
        // For some reason, the PCL wasn't supporting TakeWhile, so I skipped on this LINQ heavy solution:
        // string toPrepend = string.Concat(value.TakeWhile(c => c.Equals(' '))); // ...
        // Update: Because TakeWhile is on IEnumerable, and you need to ToCharArray on the string first. (>.<)
        // TODO: Rewrite for simplicity?
        public static Tuple<string, string> PullLeadingAndTrailingSpaces(this string value)
        {
            // I don't normally return like this. Sorry.
            if (string.IsNullOrEmpty(value))
                return new Tuple<string, string>(string.Empty, string.Empty);

            if (string.IsNullOrWhiteSpace(value))
                return new Tuple<string, string>(value, string.Empty);

            int prependCount = 0;
            int appendCount = 0;

            int i = 0;
            while (value[i++].Equals(' '))
                prependCount++;

            i = value.Length - 1;
            while (value[i--].Equals(' '))
                appendCount++;

            return new Tuple<string, string>(new string(' ', prependCount), new string(' ', appendCount));
        }
        #endregion Splits/strings into some IEnumerable

        #region Newline related
        private static char[] _acCrLF = { '\n', '\r' };

        /// <summary>
        /// Remove a trailing newline from a string.
        /// </summary>
        /// <param name="str">The string being evaluated</param>
        /// <param name="removeCRorNLifNoPlatformNL">If true, a single \n or \r, whichever is trailing,
        /// will be removed if the string does not end in an Environment.NewLine</param>
        /// <returns></returns>
        public static string RemoveLastNewLine(this string str, bool removeCRorNLifNoPlatformNL = false)
        {
            // TODO: I'm having trouble with NewLines and UWP TextBox.SelectedText, which
            // seems to blast \n characters, therefore removeCRorNLifNoPlatNL. See...
            // http://stackoverflow.com/questions/35138047/textbox-text-substringtextbox-selectionstart-doesnt-work-because-selectedtext
            if (str.EndsWith(Environment.NewLine))
            {
                str = str.Substring(0, str.Length - Environment.NewLine.Length);
            }
            else
            {
                if (str.EndsWithCRorNL() && removeCRorNLifNoPlatformNL)
                {
                    str = str.Substring(0, str.Length - 1);
                }
            }

            return str;
        }

        private static Regex _regexStartDigitsPeriod = new Regex(@"^\d+\. ");
        private static Regex _regexNoContentOL = new Regex(@"^\s*[0-9]+\. $");

        public static QuackResult QuacksLikeOrderedList(this string fullLineWithoutEnding, int tabLengthInSpaces)
        {

            QuackResult result = new QuackResult();
            var match = _regexStartDigitsPeriod.Match(fullLineWithoutEnding.TrimStart());

            int spaceCount = fullLineWithoutEnding.ToCharArray().TakeWhile(c => c.Equals(' ')).Count();

            result.QuacksLikeAnOL = 0 == spaceCount % tabLengthInSpaces
                && match.Success;

            if (match.Success)
            {
                result.HasContent = !_regexNoContentOL.IsMatch(fullLineWithoutEnding);
                result.ListOrdinal = int.Parse(match.Value.Trim().Trim('.'));
                result.MatchLength = match.Length;
                result.MatchStart = match.Index + spaceCount;
            }

            return result;
        }

        public static string RemoveLeadingNewLine(this string str)
        {
            if (str.StartsWith(Environment.NewLine))
            {
                str = str.Substring(Environment.NewLine.Length);
            }

            return str;
        }

        public static string NormalizeNewlineToCarriageReturn(this string str)
        {
            str = str.Replace("\r\n", "\r");
            str = str.Replace('\n', '\r');
            return str;
        }

        public static bool EndsWithCRorNL(this string str)
        {
            bool ret = false;

            if (!string.IsNullOrEmpty(str))
            {
                ret = _acCrLF.Contains(str[str.Length - 1]);
            }

            return ret;
        }

        public static string RemoveNewlines(this string strIn, string strReplacement)
        {
            return Regex.Replace(strIn, @"\r\n?|\n", strReplacement);
        }

        public static string SplitAtFirstNewline(this string str)
        {
            int index = str.IndexOfNewlineChar();
            return index > -1 ? str.Substring(0, index) : str;
        }

        public static string SplitAtLastNewline(this string str)
        {
            int index = str.LastIndexOfNewlineChar();
            return index > -1 ? str.Substring(index + 1) : str;
        }

        public static int IndexOfNewlineChar(this string str)
        {
            return str.IndexOfAny(_acCrLF);
        }

        public static int LastIndexOfNewlineChar(this string str)
        {
            return str.LastIndexOfAny(_acCrLF);
        }

        //public static int Last
        #endregion Newline related

        #region Counts
        public static int LengthUTF8(this string str)
        {
            return Encoding.UTF8.GetByteCount(str);
        }

        /// <summary>
        /// Checks the string to see how many times another string occurs.
        /// Case sensitive.
        /// </summary>
        /// <param name="strToSearch">The string on which this extension method is being called.</param>
        /// <param name="strToFind">The case sensitive substring whose occurrences are being counted.</param>
        /// <returns>With any luck, the number of times strToFind was found in strToSearch.</returns>
        public static int CountSubstringPatternOccurrences(this string strToSearch, string strToFind)
        {
            return strToSearch.Length - (strToSearch.Replace(strToFind, "").Length / strToFind.Length);
        }
        #endregion Counts

        #region Utility methods (I/O)
        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        // Forget doing this in a PCL, you dolt.
        //public static void WriteToClipboard(this string str)
        //{
        //    DataPackage dataPackage = new DataPackage
        //    {
        //        RequestedOperation = DataPackageOperation.Copy
        //    };
        //    dataPackage.SetText(str);
        //    Clipboard.SetContent(dataPackage);
        //}
        #endregion Utility methods (I/O)

        #region CodeProject
        // Source: http://www.codeproject.com/Articles/11902/Convert-HTML-to-Plain-Text
        // License: http://www.codeproject.com/info/cpol10.aspx
        // Note: This method has been edited from the version referenced above.
        public static string StripHTML(this string source)
        {
            try
            {
                if (string.IsNullOrEmpty(source)) return null;

                string result;

                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                result = source.Replace("\r", " ");
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = System.Text.RegularExpressions.Regex.Replace(result,
                                                                      @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*head([^>])*>", "<head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*head( )*>)", "</head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<head>).*(</head>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*script([^>])*>", "<script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*script( )*>)", "</script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty,
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<script>).*(</script>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*style([^>])*>", "<style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*style( )*>)", "</style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<style>).*(</style>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                //==============================================================
                // <li> tag custom behavior -R
                // TODO: <ol> vs. <ul>
                //==============================================================
                // TODO: This is an overly specific fix for <p>'s immediately after <li>.
                result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"<li[ ]*[^>]*>(<p[^>]*>|[^a-z0-9A-Z])*", "* ",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"</li>", "\r",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //==============================================================

                // insert tabs in spaces of <td> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*td([^>])*>", "\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI>^H^H^H^H^H tags
                // LI below now. -R
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*br( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*div([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*tr([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*p([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // TODO: Blockquote.

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < >
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<[^>]*>", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // replace special characters:
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @" ", " ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&bull;", " * ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lsaquo;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&rsaquo;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&trade;", "(tm)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&frasl;", "/",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lt;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&gt;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&copy;", "(c)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&reg;", "(r)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //================================================================================
                // More chars that I've added -R
                // TODO: Really need to call a funct each time instead of repeating so much crud.
                //================================================================================
                result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&#8217;", "'",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"(&#8220;|&#8221;)", "\"",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&#8212;", "--",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //================================================================================
                //================================================================================

                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                // Not that the above url dates this stuff at all.
                // https://web.archive.org/web/20060112044405/http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&(.{2,6});", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // for testing
                //System.Text.RegularExpressions.Regex.Replace(result,
                //       this.txtRegex.Text,string.Empty,
                //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // make line breaking consistent
                result = result.Replace("\n", "\r");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\t)", "\t\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\r)", "\t\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\t)", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove multiple tabs following a line break with just one tab
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Initial replacement target string for line breaks
                string breaks = "\r\r\r";
                // Initial replacement target string for tabs
                string tabs = "\t\t\t\t\t";
                for (int index = 0; index < result.Length; index++)
                {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }

                // That's it...
                return result.Trim();   // ... except for this trim I'm adding.
            }
            catch (Exception e)
            {
                ErrHand.LogErr(e, "StripHTML");
                throw e;
            }
        }
        #endregion CodeProject
    }

    public class QuackResult
    {
        public bool QuacksLikeAnOL = false;
        public bool HasContent = false;
        public int ListOrdinal = -1;
        public int MatchLength = -1;
        public int MatchStart = -1;

        public override string ToString()
        {
            return "Quacks? " + this.QuacksLikeAnOL
                + " :: HasContent? " + this.HasContent
                + " :: ListOrdinal: " + this.ListOrdinal
                + " :: MatchLength: " + this.MatchLength
                + " :: MatchStart: " + this.MatchStart;
        }
    }
}