//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::Xamarin.Forms.Xaml.XamlResourceIdAttribute("Pdf.controls.PenBottomBar.xaml", "controls/PenBottomBar.xaml", typeof(global::Pdf.controls.PenBottomBar))]

namespace Pdf.controls {
    
    
    [global::Xamarin.Forms.Xaml.XamlFilePathAttribute("controls\\PenBottomBar.xaml")]
    public partial class PenBottomBar : global::Xamarin.Forms.ContentView {
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Xamarin.Forms.Button colorButton;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Pdf.controls.IconView penSizeButton;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Xamarin.Forms.PancakeView.PancakeView penButtonStatus;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Xamarin.Forms.StackLayout backButton;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Pdf.controls.IconView backButtonImage;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private void InitializeComponent() {
            global::Xamarin.Forms.Xaml.Extensions.LoadFromXaml(this, typeof(PenBottomBar));
            colorButton = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.Button>(this, "colorButton");
            penSizeButton = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Pdf.controls.IconView>(this, "penSizeButton");
            penButtonStatus = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.PancakeView.PancakeView>(this, "penButtonStatus");
            backButton = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.StackLayout>(this, "backButton");
            backButtonImage = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Pdf.controls.IconView>(this, "backButtonImage");
        }
    }
}
