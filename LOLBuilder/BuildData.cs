using System;
using System.Collections.Generic;


namespace LolBuilder
{
    class BuildData
    {
        public static List<BuildInfo> BuildsList { get; set; }
        public class BuildInfo
        {
            public List<String> startingitems { get; set; }
            public List<String> buildorder { get; set; }
            public List<String> finalitems { get; set; }
        } 
    }

}
