/*
* Copyright (c) 2012 by Windward Studios, Inc. All rights reserved.
*
* This software is the confidential and proprietary information of
* Windward Studios ("Confidential Information").  You shall not
* disclose such Confidential Information and shall use it only in
* accordance with the terms of the license agreement you entered into
* with Windward Studios, Inc.
*/

/*
 * When writing your own custom functions the following data types can be
 * used for a function parameters and its return value.
 * 
 *      - object
 *      - string
 *      - double
 *      - long
 *      - arrays of the types listed above
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace WindwardCustom
{
    public class WindwardCustomFunctions
    {
        [FunctionDescription("Returns the value of Pi, 3.14159265358979, accurate to 15 digits.")]
        public static double PI()
        {
            return 3.14159265358979;
        }

        [FunctionDescription("Returns the square root of a number.")]
        public static double SQRT(
            [ParameterDescription("is the number for which you want the square root.")] double num)
        {
            return Math.Sqrt(num);
        }

        [FunctionDescription("Returns a value equal to all the values of a dataset multiplied together.")]
        public static double MULTIPLYALL(
            [ParameterDescription("is the dataset whose values you want to multiply.")] object[] nums)
        {
            if ((nums == null) || (nums.Length == 0))
                return 0.0;

            double total = 1.0;
            for (int i = 0; i < nums.Length; i++)
            {
                try
                {
                    total *= double.Parse(nums[i].ToString());
                }
                catch (Exception)
                {
                    // Ignore it if we've got not a number.
                }
            }

            return total;
        }
    } // class WindwardCustomFunctions
} // namespace WindwardCustom
