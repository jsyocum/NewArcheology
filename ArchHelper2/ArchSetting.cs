using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ArchHelper2.MainWindow;
using static ArchHelper2.CSharpHelper;
using static ArchHelper2.XAMLHelper;
using static ArchHelper2.DebugConsole;
using static ArchHelper2.DebugConsoleTools;
using static ArchHelper2.Artefacts;
using static ArchHelper2.Materials;

namespace ArchHelper2
{
    public class ArchSetting
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int ValueAsInt { get; set; }
        public double ValueAsDouble { get; }
        public ArchSetting(string name, string value)
        {
            Name = name;
            Value = value;
            ValueAsInt = StringToInt(value);
            ValueAsDouble = StringToDouble(Value);
        }
        
        public ArchSetting(string name, int value)
        {
            Name = name;
            Value = value.ToString();
            ValueAsInt = value;
            ValueAsDouble = (double)value;
        }

        public ArchSetting(string name, double value)
        {
            Name = name;
            Value = value.ToString();
            ValueAsInt = (int)value;
            ValueAsDouble = value;
        }

        public ArchSetting(string name)
        {
            Name = name;
            Value = "null";

        }

        public ArchSetting()
        {
            Name = "null";
            Value = "null";
            ValueAsDouble = StringToDouble(Value);
        }

        public ArchSetting returnArchSetting()
        {
            ArchSetting archSetting = new ArchSetting(Name, Value);
            return archSetting;
        }

        public void Update(List<ArchSetting> archSettings)
        {
            List<ArchSetting> archSettingsCopy = new List<ArchSetting>(archSettings);

            foreach (ArchSetting archSetting in archSettingsCopy)
            {
                if (Name == archSetting.Name)
                {
                    archSettings.Remove(archSetting);
                }
            }

            archSettings.Add(returnArchSetting());
        }

        public void Update()
        {
            Update(settings);
        }
    }
}