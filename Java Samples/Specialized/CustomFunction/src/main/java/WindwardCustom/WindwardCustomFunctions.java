/*
* Copyright (c) 2010 by Windward Studios, Inc. All rights reserved.
*
* This software is the confidential and proprietary information of
* Windward Studios ("Confidential Information").  You shall not
* disclose such Confidential Information and shall use it only in
* accordance with the terms of the license agreement you entered into
* with Windward Studios, Inc.
*/
package WindwardCustom;

/**
 * Class containing custom functions for the Windward reporting solution
 * Default implementation
 */
import net.windward.util.macro.IMacroState;
import net.windward.xmlreport.WindwardEventHandler;

public class WindwardCustomFunctions
{
	public static int numberOfFunctions = 3;
	public static WindwardEventHandler eventHandler;
	public static String[] functionName = new String[numberOfFunctions];
	public static String[] functionFullName = new String[numberOfFunctions];
	public static String[] functionDescription = new String[numberOfFunctions];
	public static Integer[] functionNumberOfArgument = new Integer[numberOfFunctions];
	public static String[][] functionArgumentName = new String[numberOfFunctions][];
	public static String[][] functionArgumentDescription = new String[numberOfFunctions][];
	public static String[][] functionArgumentType = new String[numberOfFunctions][];

	static
	{
		functionName[0] = "PI";
		functionFullName[0] = "PI()";
		functionDescription[0] = "Returns the value of Pi, 3.14159265358979, accurate to 15 digits.";
		functionNumberOfArgument[0] = new Integer(0);
		functionArgumentName[0] = null;
		functionArgumentDescription[0] = null;
		functionArgumentType[0] = null;

		functionName[1] = "SQRT";
		functionFullName[1] = "SQRT(number)";
		functionDescription[1] = "Returns the square root of a number.";
		functionNumberOfArgument[1] = new Integer(1);
		functionArgumentName[1] = new String[] { "Number" };
		functionArgumentDescription[1] = new String[] { "is the number for which you want the square root." };
		functionArgumentType[1] = new String[] { "number" };

		functionName[2] = "MULTIPLYALL";
		functionFullName[2] = "MULTIPLYALL(dataset)";
		functionDescription[2] = "Returns a value equal to all the values of a dataset multiplied together.";
		functionNumberOfArgument[2] = new Integer(1);
		functionArgumentName[2] = new String[] { "Dataset" };
		functionArgumentDescription[2] = new String[] { "is the dataset whose values you want to multiply." };
		functionArgumentType[2] = new String[] { "dataset" };

		/*
		ADDTOTAL and GETTOTAL are built-in engine macros now.
		These are left here for the reference on how to use the macro state.
		*/
		/*
		functionName[3] = "ADDTOTAL";
		functionFullName[3] = "ADDTOTAL(number,text)";
		functionDescription[3] = "Adds number to running total";
		functionNumberOfArgument[3] = new Integer(2);
		functionArgumentName[3] = new String[] { "Number","Key" };
		functionArgumentDescription[3] = new String[] { "Is the number you want to add to the running total.","Is the name of the running total to use" };
		functionArgumentType[3] = new String[] { "number","text" };

		functionName[4] = "GETTOTAL";
		functionFullName[4] = "GETTOTAL(text)";
		functionDescription[4] = "Get number of running total";
		functionNumberOfArgument[4] = new Integer(1);
		functionArgumentName[4] = new String[] { "Key" };
		functionArgumentDescription[4] = new String[] { "Is the name of the running total you want to return" };
		functionArgumentType[4] = new String[] { "text" };
		*/
	}

	public static Object PI()
	{
		return new Double(3.14159265358979);
	}

	public static Object SQRT(Number num)
	{
		return new Double(Math.sqrt(num.doubleValue()));
	}

	public static Object MULTIPLYALL(Object nums[])
	{
		if ((nums == null) || (nums.length == 0))
			return new Double(0);

		Double total = new Double(1);
		for (int i = 0; i < nums.length; i++)
		{
			if (nums[i] instanceof Number)
				total = new Double(total.longValue() * ((Number)nums[i]).doubleValue());
		}
		return total;
	}

	/*
	ADDTOTAL and GETTOTAL are built-in engine macros now.
	These are left here for the reference on how to use the macro state.
	*/
	/*
	public static Object ADDTOTAL(Number num, String key)
	{
		Double retVal;
		if (eventHandler == null)
			return "";
		if (eventHandler.getData(key) != null)
			retVal = new Double(((Number) eventHandler.getData(key)).doubleValue() + (num.doubleValue()));
		else
			retVal = new Double(num.doubleValue());
		eventHandler.setData(key, retVal);
		return "";
	}

	public static Object GETTOTAL(String key)
	{
		if (eventHandler == null || eventHandler.getData(key) == null)
			return "";
		return ((Double) eventHandler.getData(key));
	}
	*/

	public static void SetMacroState(IMacroState state)
	{
		eventHandler =  state.getEventHandler();
	}
}