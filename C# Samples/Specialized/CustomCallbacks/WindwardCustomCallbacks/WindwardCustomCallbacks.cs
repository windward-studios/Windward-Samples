using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using net.windward.datasource;
using net.windward.env;
using net.windward.tags;

namespace WindwardCallback
{
    /**
    * Class containing Windward custom user callbacks
    * Build project using Visual Studio. Leave version number and key the same or it will not work
     * Copy 'WindwardCustomCallbacks.dll' into your windwardreports lib directory or Autotag lib directory to use
    * Only Version 13.0 and later
    */

    public class WindwardCustomCallbacks {
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
        public static Object ApproveSelect(String select, DataSourceProvider provider, BaseTag xmlTag) {

            if (select.Equals("$$THIS_IS_INCORRECTLY_FORMATTED$$")) //In a custom callback, you can modify the select tag.
                return "$$THIS_IS_CORRECTLY_FORMMATED$$";
            //We can also prevent people from accessing unwanted data, such as, perhaps, the salary node of a database
            if (select.ToLower().Contains("salary"))
                throw new DataSourceException("Select for this tag was denied by custom callback because we don't want someone seeing our salary", 0);
            return select;
        }
    }
}
