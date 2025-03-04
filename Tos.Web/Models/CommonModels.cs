using System;

namespace TOS.Web.Models
{
    public class CommonMasterModelRequest
    {
        public string cd_category { get; set; }
        public string cd_common { get; set; }
        public short? flg_use { get; set; }
    }

    public class CommonModelResponse
    {
        public short value { get; set; }
        public string name { get; set; }
        public short cd_sort { get; set; }
        public bool? isDisabled { get; set; }
        public bool? disabled { get; set; }
        public bool? isChecked { get; set; }
        public bool? selected { get; set; }

        public int? selectedIndex { get; set; }

        public CommonModelResponse() { }

        public CommonModelResponse(short value, string name, bool? isDisabled = null, bool? isChecked = null)
        {
            this.value = value;
            this.name = name;
            this.isDisabled = isDisabled;
            this.isChecked = isChecked;
        }

        public CommonModelResponse(short value, string name, bool? isDisabled = null, bool? selected = null, bool? isWithSelect = null)
        {
            this.value = value;
            this.name = name;
            this.isDisabled = isDisabled;
            this.selected = selected;
        }
    }
    

    public class inputMultiselectAutocompleteModel : CommonModelResponse
    {
        public short? flg_use { get; set; }
    }

    public class ComboboxModelRequest : CommonMasterModelRequest
    {
        public string nameJson { get; set; }
    }

    public class DataImage
    {
        public object nm_file_path { get; set; }
        public string cd_file { get; set; }
        public bool isNew { get; set; }
        public string base64 { get; set; }
    }
     public class DateUpdate
    {
        public string name { get; set; }
        public DateTime? value { get; set; }
    }
    public class Error
    {
        public string RecordNumber { get; set; }
        public string ColumnName { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorValue { get; set; }
    }


}