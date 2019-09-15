using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ComboBox
{
    public class ComboxItemBindingViewModel : DotvvmViewModelBase
    {

        public async override Task Load()
        {
            if (!Context.IsPostBack)
            {
                ComplexData = Enumerable.Range(1, 10)
                    .Select(s => new ComplexType {
                        Id = s, Text = $"Text {s}", Date = new DateTime(2019, 10, s), NestedComplex = new NestedComplexType { Text2 = $"Nested text {s}" }
                    }).ToList();
            }
            await base.Load();
        }


        public List<ComplexType> ComplexData { get; set; }

        public class ComplexType
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public DateTime Date { get; set; }
            public NestedComplexType NestedComplex { get; set; }

        }
        public class NestedComplexType
        {
            public string Text2 { get; set; }
        }

        public object SelectedValue { get; set; }
    }
}

