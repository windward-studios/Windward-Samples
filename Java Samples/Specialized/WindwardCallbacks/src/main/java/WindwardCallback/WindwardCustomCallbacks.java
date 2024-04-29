/*
* Copyright (c) 2010 by Windward Studios, Inc. All rights reserved.
*
* This software is the confidential and proprietary information of
* Windward Studios ("Confidential Information").  You shall not
* disclose such Confidential Information and shall use it only in
* accordance with the terms of the license agreement you entered into
* with Windward Studios, Inc.
*/
package WindwardCallback;

import net.windward.datasource.DataSourceProvider;
import net.windward.env.DataSourceException;
import net.windward.tags.BaseTag;

/**
 * Class containing Windward custom user callbacks
 * Build jar from IntelliJ by going to Build menu then click build artifacts then hit build once more on the popup menu
 * Copy to your WindwardReports jar lib directory and overwrite the default one if it is there
 * Only Version 13.0 and later
 */

public class WindwardCustomCallbacks
{
    /**
     * This is a sample function that will check to see if the user is accessing the salary node in our customCallback database.
     * If they are, then it throws a new DataSourceException. It also does other things for the sake of being a sample, too.
     * If you modify this, make sure to keep the method name the same.
     *
     * @param select The select statement
     * @param provider The datasource provider.
     * @param xmlTag The xmlTag
     * @return
     * @throws DataSourceException
     */
    public static Object ApproveSelect(String select, DataSourceProvider provider, BaseTag xmlTag) throws DataSourceException {

        if(select.equals("$$THIS_IS_INCORRECTLY_FORMATTED$$")) //In a custom callback, you can modify the select tag.
            return "$$THIS_IS_CORRECTLY_FORMMATED$$";
        //We can also prevent people from accessing unwanted data, such as, perhaps, the salary node of a database
        if(select.toLowerCase().contains("salary"))
            throw new DataSourceException("Select for this tag was denied by custom callback because we don't want someone seeing our salary",0);
        return select;
    }
}
