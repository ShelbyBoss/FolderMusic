﻿using System;
using System.Xml.Serialization;

namespace FolderMusic.FrameHistory
{
    public class HistoricFrame
    {
        private string pageTypeName;

        public string PageTypeName
        {
            get { return pageTypeName; }
            set
            {
                pageTypeName = value;
                Page = Type.GetType(value);

                if (PageTypeName == "MobileDebug.DebugPage") Page = typeof(MobileDebug.DebugPage);
                if (PageTypeName == "MobileDebug.DebugFilterPage") Page = typeof(MobileDebug.DebugFilterPage);
            }
        }

        [XmlIgnore]
        public Type Page { get; private set; }

        public HistoricParameter Parameter { get; set; }

        public HistoricFrame()
        {
        }

        public HistoricFrame(Type page, HistoricParameter parameter)
        {
            pageTypeName = page.FullName;
            Page = page;
            Parameter = parameter;
        }
    }
}
