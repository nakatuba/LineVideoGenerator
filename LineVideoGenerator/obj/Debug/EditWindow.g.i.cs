﻿#pragma checksum "..\..\EditWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "92EC3B67327734A30C26E700E08C6B61A9B5E6AAD75418EC7E1713F2F359D2D1"
//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace LineVideoGenerator {
    
    
    /// <summary>
    /// EditWindow
    /// </summary>
    public partial class EditWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 9 "..\..\EditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox myTextBox;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\EditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button mySendButton;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\EditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button iconButton;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\EditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox nameTextBox;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\EditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox otherTextBox;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\EditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button otherSendButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/LineVideoGenerator;component/editwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\EditWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 7 "..\..\EditWindow.xaml"
            ((LineVideoGenerator.EditWindow)(target)).Closed += new System.EventHandler(this.Window_Closed);
            
            #line default
            #line hidden
            return;
            case 2:
            this.myTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 18 "..\..\EditWindow.xaml"
            this.myTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.MyTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.mySendButton = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\EditWindow.xaml"
            this.mySendButton.Click += new System.Windows.RoutedEventHandler(this.SendButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.iconButton = ((System.Windows.Controls.Button)(target));
            
            #line 35 "..\..\EditWindow.xaml"
            this.iconButton.Click += new System.Windows.RoutedEventHandler(this.IconButton_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.nameTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 54 "..\..\EditWindow.xaml"
            this.nameTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.OtherTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.otherTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 64 "..\..\EditWindow.xaml"
            this.otherTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.OtherTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 7:
            this.otherSendButton = ((System.Windows.Controls.Button)(target));
            
            #line 73 "..\..\EditWindow.xaml"
            this.otherSendButton.Click += new System.Windows.RoutedEventHandler(this.SendButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
