using System;
using System.Collections.Generic;
using System.Dynamic;


namespace LolBuilder
{
    class BuildData
    {
        public static List<BuildInfo> BuildsList { get; set; }
        public static int[] SkillSequence { get; set; }
        public static List<int[]> SkillSequenceList { get; set; }
 
        public class BuildInfo
        {
            public List<String> startingitems { get; set; }
            public List<String> buildorder { get; set; }
            public List<String> finalitems { get; set; }
        } 
    }

}
