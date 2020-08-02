using Android.Content;
using Pdf.controls;
using Pdf.Enumerations;
using Pdf.Interfaces;
using Pdf.Models;
using Pdf.ViewModels;
using Syncfusion.SfPdfViewer.XForms;
using Syncfusion.XForms.PopupLayout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Acr.UserDialogs;
using SlideOverKit;
using System.Collections.ObjectModel;
using System.Threading;

namespace Pdf.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PdfViewer : MenuContainerPage, INotifyPropertyChanged
    {
        #region Private Members

        private Stream pdfStream;
        private LoadingMode loadingMode;

        private PdfViewerModel pdfViewerModel;
        private SfPopupLayout stylePopup;
        private SfPopupLayout errorSearchPopup;
        private SfPopupLayout passwordPopup;
        private SfPopupLayout popupMenu;
        private StampSlideUpMenu stampSlideUpMenu;

        private StyleContent styleContent;
        private PopupMenuContent popupMenuContent;
        private SearchErrorPopup searchErrorPopupContent;

        private DataTemplate styleTemplateStylePopup;
        private DataTemplate styleTemplateErrorPopup;
        private DataTemplate templateViewSetPasswordPopup;

        private ImageButton image;
        private Label label;
        private Entry entry;
        private Button acceptButton;
        private Button declineButton;

        private FreeTextAnnotation selectedFreeTextAnnotation;
        private InkAnnotation selectedInkAnnotation;
        private HandwrittenSignature selectedHandwrittenSignature;
        private ShapeAnnotation selectedShapeAnnotation;
        private TextMarkupAnnotation selectedTextMarkupAnnotation;
        private StampAnnotation selectedStampAnnotation;

        private Color selectedColor = Color.Black;
        private AnnotationType annotationType;

        private readonly string filePath;

        private bool canSaveDocument = false;
        private bool canUndoInk = false;
        private bool canRedoInk = false;
        private bool toolbarIsCollapsed = false;
        private bool searchStarted = false;
        private bool isATryToOpenTheDocument = false;
        private bool hasAlreadyTriedToOpenDoc = false;
        private bool isPasswordPopupInitiInitalized = false;
        private bool isInkMenuEnabled = false;
        private bool isFreeTextMenuEnabled = false;
        private bool isShapeMenuEnabled = false;
        private bool isTextMarkupMenuEnabled = false;
        private bool isHandwrittenSignatureMenuEnabled = false;

        private int fontSize = 6;
        private int shapesNumbers;
        private int textMarkupNumbers;
        private int lastThicknessBarSelected = 5;
        private int lastOpacitySelected = 4;
        private int numberOfAnnotation = 0;

        public List<byte[]> ThumbnailBytes { get; set; }
        public ObservableCollection<byte[]> ThumbnailInfoCollection { get; set; }
        #endregion

        #region Property

        public bool IsTextMarkupMenuEnabled
        {
            get => isTextMarkupMenuEnabled;

            set
            {
                isTextMarkupMenuEnabled = value;
                OnPropertyChanged();

                if (isTextMarkupMenuEnabled == false)
                {
                    BackButtonAnnotationTypeToolbar_Clicked(null, null);
                }
                else
                {
                    switch (annotationType)
                    {
                        case AnnotationType.Hightlight:
                            HightlightButton_Clicked(null, null);
                            break;
                        case AnnotationType.Underline:
                            UnderlineButton_Clicked(null, null);
                            break;
                        case AnnotationType.Strikethrought:
                            StrikethroughtButton_Clicked(null, null);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public bool IsShapeMenuEnabled
        {
            get
            {
                return isShapeMenuEnabled;
            }
            set
            {
                isShapeMenuEnabled = value;
                OnPropertyChanged();

                if (isShapeMenuEnabled == false)
                {
                    BackButtonAnnotationTypeToolbar_Clicked(null, null);
                }
                else
                {
                    switch (annotationType)
                    {
                        case AnnotationType.Rectangle:
                            RectangleButton_Clicked(null, null);
                            break;
                        case AnnotationType.Line:
                            LineButton_Clicked(null, null);
                            break;
                        case AnnotationType.Arrow:
                            ArrowButton_Clicked(null, null);
                            break;
                        case AnnotationType.Circle:
                            CircleButton_Clicked(null, null);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public bool IsInkMenuEnabled
        {
            get => isInkMenuEnabled;

            set
            {
                isInkMenuEnabled = value;
                OnPropertyChanged();

                if (isInkMenuEnabled == false)
                {
                    BackButtonAnnotationTypeToolbar_Clicked(null, null);
                }
                else
                {
                    SetToolbarForInkAnnotationSelected();
                }
            }
        }

        public bool IsHandwrittenSignatureMenuEnabled
        {
            get => isHandwrittenSignatureMenuEnabled;

            set
            {
                isHandwrittenSignatureMenuEnabled = value;
                OnPropertyChanged();

                if (isHandwrittenSignatureMenuEnabled == false)
                {
                    BackButtonAnnotationTypeToolbar_Clicked(null, null);
                }
                else
                {
                    SetToolbarForHandwrittenSignatureSelected();
                }
            }
        }

        public bool IsFreeTextMenuEnabled
        {
            get => isFreeTextMenuEnabled;

            set
            {
                isFreeTextMenuEnabled = value;
                OnPropertyChanged();

                if (isFreeTextMenuEnabled == false)
                {
                    BackButtonAnnotationTypeToolbar_Clicked(null, null);
                }
                else
                {
                    SetToolbarForFreeTextAnnotationSelected();
                }
            }
        }

        public int NumberOfAnnotation
        {
            get => numberOfAnnotation;

            set
            {
                numberOfAnnotation = value;
                OnPropertyChanged();

                if (numberOfAnnotation != 0)
                    CanSaveDocument = true;
                else
                {
                    CanSaveDocument = false;
                }
            }
        }

        public bool CanSaveDocument
        {
            get => canSaveDocument;

            set
            {
                canSaveDocument = value;
                OnPropertyChanged();

                ItemsMenu item = (ItemsMenu)popupMenuContent.ItemsMenu[0];

                if (canSaveDocument == true)
                {
                    item.TextColor = Color.FromHex("#616161");
                    item.ImageColor = Color.FromHex("#373737");
                }
                else
                {
                    item.TextColor = Color.FromHex("#e0e0e0");
                    item.ImageColor = Color.FromHex("#707070");
                }
            }
        }

        public SfPdfViewer PdfViewerControl
        {
            get => pdfViewerControl;

            set => pdfViewerControl = value;
        }

        public Color SelectedColor
        {
            get => selectedColor;

            set
            {
                selectedColor = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();

                if (selectedFreeTextAnnotation != null)
                    selectedFreeTextAnnotation.Settings.TextColor = value;

                if (selectedInkAnnotation != null)
                    selectedInkAnnotation.Settings.Color = value;

                if (selectedShapeAnnotation != null)
                    selectedShapeAnnotation.Settings.StrokeColor = value;

                if (selectedTextMarkupAnnotation != null)
                    selectedTextMarkupAnnotation.Settings.Color = value;

                switch (pdfViewerControl.AnnotationMode)
                {
                    case AnnotationMode.None:
                        break;
                    case AnnotationMode.Highlight:
                        pdfViewerControl.AnnotationSettings.TextMarkup.Highlight.Color = value;
                        break;
                    case AnnotationMode.Underline:
                        pdfViewerControl.AnnotationSettings.TextMarkup.Underline.Color = value;
                        break;
                    case AnnotationMode.Ink:
                        pdfViewerControl.AnnotationSettings.Ink.Color = value;
                        break;
                    case AnnotationMode.Strikethrough:
                        pdfViewerControl.AnnotationSettings.TextMarkup.Strikethrough.Color = value;
                        break;
                    case AnnotationMode.FreeText:
                        pdfViewerControl.AnnotationSettings.FreeText.TextColor = value;
                        break;
                    case AnnotationMode.Rectangle:
                        pdfViewerControl.AnnotationSettings.Rectangle.Settings.StrokeColor = value;
                        break;
                    case AnnotationMode.Circle:
                        pdfViewerControl.AnnotationSettings.Circle.Settings.StrokeColor = value;
                        break;
                    case AnnotationMode.Line:
                        pdfViewerControl.AnnotationSettings.Line.Settings.StrokeColor = value;
                        break;
                    case AnnotationMode.Arrow:
                        pdfViewerControl.AnnotationSettings.Arrow.Settings.StrokeColor = value;
                        break;
                    case AnnotationMode.HandwrittenSignature:
                        break;
                    default:
                        break;
                }
            }
        }

        public int FontSize
        {
            get => fontSize;

            set
            {
                fontSize = value;
                OnPropertyChanged();

                if (selectedFreeTextAnnotation != null)
                    selectedFreeTextAnnotation.Settings.TextSize = value;

                if (pdfViewerControl.AnnotationMode == AnnotationMode.FreeText)
                    pdfViewerControl.AnnotationSettings.FreeText.TextSize = value;
            }
        }

        public bool CanUndoInk
        {
            get => canUndoInk;

            set
            {
                canUndoInk = value;
                OnPropertyChanged();

                if (value == true)
                {
                    UndoButton.Foreground = Color.FromHex("#373737");
                    ValidButton.Foreground = Color.FromHex("#373737");
                }
                else
                {
                    UndoButton.Foreground = Color.Transparent;
                    ValidButton.Foreground = Color.Transparent;
                }
            }
        }

        public bool CanRedoInk
        {
            get => canRedoInk;

            set
            {
                canRedoInk = value;
                OnPropertyChanged();

                RedoButton.Foreground = value 
                    ? Color.FromHex("#373737") 
                    : Color.Transparent;
            }
        }

        public int ShapesNumbers
        {
            get => shapesNumbers;

            set
            {
                shapesNumbers = value;
                OnPropertyChanged();

                //TODO -- Check later If we need to keep this two lines

                if (ShapesNumbers == 0)
                    ValidButton.Foreground = Color.FromHex("#373737");
                else
                    ValidButton.Foreground = Color.FromHex("#373737");
            }
        }

        public int TextMarkupNumbers
        {
            get => textMarkupNumbers;

            set
            {
                textMarkupNumbers = value;
                OnPropertyChanged();

                //TODO -- Check later If we need to keep this two lines

                if (TextMarkupNumbers == 0)
                    ValidButton.Foreground = Color.FromHex("#373737");
                else
                    ValidButton.Foreground = Color.FromHex("#373737");
            }
        }
        #endregion

        #region On Disappearing
        protected override void OnDisappearing()
        {
            UnsubscribeFromEvents();

            if (Device.RuntimePlatform == Device.Android)
                DependencyService.Get<IThemeManager>().SetMenuStatusBarColor();

            pdfViewerControl.Unload();

            base.OnDisappearing();
        }

        private void UnsubscribeFromEvents()
        {
            MessagingCenter.Unsubscribe<ColorPicker, Xamarin.Forms.Color>(this, "selectedColor");

            pdfViewerControl.FreeTextAnnotationAdded -= PdfViewerControl_FreeTextAnnotationAdded;
            pdfViewerControl.FreeTextAnnotationSelected -= PdfViewerControl_FreeTextAnnotationSelected;
            pdfViewerControl.FreeTextAnnotationDeselected -= PdfViewerControl_FreeTextAnnotationDeselected;

            pdfViewerControl.CanRedoInkModified -= PdfViewerControl_CanRedoInkModified;
            pdfViewerControl.CanUndoInkModified -= PdfViewerControl_CanUndoInkModified;

            pdfViewerControl.InkSelected -= PdfViewerControl_InkSelected;
            pdfViewerControl.InkDeselected -= PdfViewerControl_InkDeselected;

            pdfViewerControl.ShapeAnnotationAdded -= PdfViewerControl_ShapeAnnotationAdded;
            pdfViewerControl.ShapeAnnotationSelected -= PdfViewerControl_ShapeAnnotationSelected;
            pdfViewerControl.ShapeAnnotationDeselected -= PdfViewerControl_ShapeAnnotationDeselected;

            pdfViewerControl.TextMarkupSelected -= PdfViewerControl_TextMarkupSelected;
            pdfViewerControl.TextMarkupDeselected -= PdfViewerControl_TextMarkupDeselected;
            pdfViewerControl.TextMarkupAdded -= PdfViewerControl_TextMarkupAdded;

            pdfViewerControl.DocumentLoaded -= PdfViewerControl_DocumentLoaded;

            pdfViewerControl.TextMarkupAdded -= PdfViewerControl_TextMarkupAdded;
            pdfViewerControl.PageChanged -= PdfViewerControl_PageChanged;
            pdfViewerControl.SearchCompleted -= PdfViewerControl_SearchCompleted;
            pdfViewerControl.TextMatchFound -= PdfViewerControl_TextMatchFound;
            pdfViewerControl.SearchInitiated -= PdfViewerControl_SearchInitiated;
            pdfViewerControl.StampAnnotationAdded -= PdfViewerControl_StampAnnotationAdded;
            pdfViewerControl.StampAnnotationSelected -= PdfViewerControl_StampAnnotationSelected;
            pdfViewerControl.StampAnnotationDeselected -= PdfViewerControl_StampAnnotationDeselected;

            pdfViewerControl.DoubleTapped -= PdfViewerControl_DoubleTapped;

            styleContent.ThicknessBar.BoxViewButtonClicked -= ThicknessBar_Clicked;
            styleContent.OpacityButtonClicked -= OpacityIcon_Clicked;

            stampSlideUpMenu.StampListView.SelectionChanged -= StampListView_SelectionChanged;

            if (passwordPopup != null)
            {
                passwordPopup.Closed -= PasswordPopup_Closed;
                acceptButton.Clicked -= AcceptButtonPasswordPopup_Clicked;
                declineButton.Clicked -= DeclineButton_Clicked;

                passwordPopup.IsOpen = false;
            }
        }

        #endregion

        #region On Appearing 

        protected override void OnAppearing()
        {
            // I disabled the nav bar here because the nav bar will be removed
            // before the document start loading -> Bad for UI
            Shell.SetNavBarIsVisible(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            if (loadingMode == LoadingMode.ByIntent)
            {
                // I disabled the tab bar here because the nav bar will be removed
                // before the document start loading -> Bad for UI
                Shell.SetTabBarIsVisible(this, false);

                if (Device.RuntimePlatform == Device.Android)
                    DependencyService.Get<IThemeManager>().SetPdfViewerStatusBarColor();
            }
            else
            {
                UserDialogs.Instance.ShowLoading("Loading", MaskType.Black);
            }

            new Task(LoadPdf).Start();

            base.OnAppearing();
        }
        #endregion

        private async void LoadPdf()
        {
            if (loadingMode == LoadingMode.ByIntent)
                UserDialogs.Instance.ShowLoading("Loading", MaskType.Black);

            if (Device.RuntimePlatform == Device.Android)
            {
                pdfStream = DependencyService.Get<IAndroidFileHelper>().GetFileStream(filePath);
               
                // PDFium renderer
                pdfViewerControl.CustomPdfRenderer = DependencyService.Get<ICustomPdfRendererService>().AlternatePdfRenderer;
            }

            await Task.Run(async () =>
            {
                await pdfViewerControl.LoadDocumentAsync(pdfStream, new CancellationTokenSource());
            });
        }

        #region Constructor
        public PdfViewer(string filePath, LoadingMode loadingMode)
        {
            InitializeComponent();

            this.loadingMode = loadingMode;
            this.filePath = filePath;
            ThumbnailBytes = new List<byte[]>();
            ThumbnailInfoCollection = new ObservableCollection<byte[]>();

            //Disable the display the default toolbar
            pdfViewerControl.IsToolbarVisible = false;

            //Disable the display of password UI view
            pdfViewerControl.IsPasswordViewEnabled = false;

            stampSlideUpMenu = new StampSlideUpMenu();
            this.SlideMenu = stampSlideUpMenu;
            stampSlideUpMenu.StampListView.SelectionChanged += StampListView_SelectionChanged;

            pdfViewerControl.PasswordErrorOccurred += PdfViewerControl_PasswordErrorOccurred;

            pdfViewerControl.DocumentLoaded += PdfViewerControl_DocumentLoaded;

            MessagingCenter.Subscribe<ColorPicker, Xamarin.Forms.Color>(this, "selectedColor", (sender, helper) =>
            {
                this.SelectedColor = helper;
            });
        }
        #endregion

        #region On Document Loaded Event Handler
        private void PdfViewerControl_DocumentLoaded(object sender, EventArgs args)
        {
            if (isPasswordPopupInitiInitalized != true)
            {
                Shell.SetNavBarIsVisible(this, false);
                NavigationPage.SetHasNavigationBar(this, false);
            }

            if (Device.RuntimePlatform == Device.Android)
                DependencyService.Get<IThemeManager>().SetPdfViewerStatusBarColor();

            Shell.SetTabBarIsVisible(this, false);
            Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);

            SubscribeToPdfViewerEvents();

            pdfViewerControl.PreserveSignaturePadOrientation = true;

            InitializeStylePopup();

            InitializeSearchErrorPopup();

            InitializePopupMenu();

            SetBindingContext();

            annotationType = AnnotationType.None;

            UserDialogs.Instance.HideLoading();
        }

        private void SetBindingContext()
        {
            paletteButton.BindingContext = this;
            trashButton.BindingContext = this;
            styleContent.BindingContext = this;
        }

        private void InitializeStylePopup()
        {
            this.styleContent = new StyleContent();
            stylePopup = new SfPopupLayout();

            styleContent.ThicknessBar.BindingContext = this;
            stylePopup.ClosePopupOnBackButtonPressed = false;
            stylePopup.PopupView.ShowHeader = false;
            stylePopup.PopupView.ShowFooter = false;
            stylePopup.PopupView.HeightRequest = 192;
            stylePopup.PopupView.WidthRequest = 280;
            stylePopup.PopupView.PopupStyle.BorderColor = Color.FromHex("#fafafa");
            stylePopup.PopupView.AnimationMode = AnimationMode.SlideOnBottom;

            styleTemplateStylePopup = new DataTemplate(() => styleContent);

            this.stylePopup.PopupView.ContentTemplate = styleTemplateStylePopup;

            styleContent.ThicknessBar.BoxViewButtonClicked += ThicknessBar_Clicked;
            styleContent.OpacityButtonClicked += OpacityIcon_Clicked;
        }

        private void InitializeSearchErrorPopup()
        {
            searchErrorPopupContent = new SearchErrorPopup();

            errorSearchPopup = new SfPopupLayout
            {
                PopupView = {ShowHeader = false, ShowFooter = false, HeightRequest = 105, WidthRequest = 300},
                BackgroundColor = Color.FromHex("#fafafa")
            };
            errorSearchPopup.PopupView.BackgroundColor = Color.FromHex("#fafafa");
            errorSearchPopup.PopupView.PopupStyle.BorderColor = Color.FromHex("#fafafa");
            errorSearchPopup.PopupView.AnimationMode = AnimationMode.Fade;

            styleTemplateErrorPopup = new DataTemplate(() => searchErrorPopupContent);

            this.errorSearchPopup.PopupView.ContentTemplate = styleTemplateErrorPopup;

            if (passwordPopup != null)
                passwordPopup.IsOpen = false;
        }

        private void InitializePopupMenu()
        {
            popupMenu = new SfPopupLayout
            {
                PopupView =
                {
                    IsFullScreen = true,
                    AnimationDuration = 200,
                    AnimationMode = AnimationMode.SlideOnBottom,
                    PopupStyle = {CornerRadius = 0, BorderThickness = 2, BorderColor = Color.White},
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand
                },
                Padding = new Thickness(15, 15, 15, 15)
            };
            popupMenu.PopupView.ShowFooter = false;
            popupMenu.PopupView.ShowCloseButton = false;
            popupMenu.PopupView.PopupStyle.HeaderBackgroundColor = Color.FromHex("#eeeeee");
            popupMenu.PopupView.PopupStyle.BorderColor = Color.FromHex("#e0e0e0");

            popupMenuContent = new PopupMenuContent();

            var popupMenuContentTemplate = new DataTemplate(() => popupMenuContent);

            var headerTemplateViewPopupMenu = new DataTemplate(() =>
            {
                var stackLayout = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal, 
                    Padding = new Thickness(0, 0, 13, 0)
                };

                var title = new Label
                {
                    Text = "More",
                    FontSize = 18,
                    FontFamily = "GothamBold.ttf#GothamBold",
                    VerticalTextAlignment = Xamarin.Forms.TextAlignment.Center,
                    HorizontalTextAlignment = Xamarin.Forms.TextAlignment.Center,
                    TextColor = Color.FromHex("#4e4e4e"),
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };

                image = new ImageButton
                {
                    Source = "outlineClearViewer.xml",
                    Aspect = Aspect.AspectFit,
                    BackgroundColor = Color.Transparent,
                    HorizontalOptions = LayoutOptions.End
                };

                stackLayout.Children.Add(title);
                stackLayout.Children.Add(image);

                return stackLayout;
            });

            popupMenu.PopupView.HeaderTemplate = headerTemplateViewPopupMenu;

            popupMenu.PopupView.ContentTemplate = popupMenuContentTemplate;
        }

        private void SubscribeToPdfViewerEvents()
        {
            pdfViewerControl.FreeTextAnnotationAdded += PdfViewerControl_FreeTextAnnotationAdded;
            pdfViewerControl.FreeTextAnnotationSelected += PdfViewerControl_FreeTextAnnotationSelected;
            pdfViewerControl.FreeTextAnnotationDeselected += PdfViewerControl_FreeTextAnnotationDeselected;

            pdfViewerControl.CanRedoInkModified += PdfViewerControl_CanRedoInkModified;
            pdfViewerControl.CanUndoInkModified += PdfViewerControl_CanUndoInkModified;

            pdfViewerControl.InkSelected += PdfViewerControl_InkSelected;
            pdfViewerControl.InkDeselected += PdfViewerControl_InkDeselected;
            pdfViewerControl.InkAdded += PdfViewerControl_InkAdded;

            pdfViewerControl.ShapeAnnotationAdded += PdfViewerControl_ShapeAnnotationAdded;
            pdfViewerControl.ShapeAnnotationSelected += PdfViewerControl_ShapeAnnotationSelected;
            pdfViewerControl.ShapeAnnotationDeselected += PdfViewerControl_ShapeAnnotationDeselected;

            pdfViewerControl.TextMarkupSelected += PdfViewerControl_TextMarkupSelected;
            pdfViewerControl.TextMarkupDeselected += PdfViewerControl_TextMarkupDeselected;
            pdfViewerControl.TextMarkupAdded += PdfViewerControl_TextMarkupAdded;

            pdfViewerControl.PageChanged += PdfViewerControl_PageChanged;
            pdfViewerControl.SearchCompleted += PdfViewerControl_SearchCompleted;
            pdfViewerControl.TextMatchFound += PdfViewerControl_TextMatchFound;
            pdfViewerControl.SearchInitiated += PdfViewerControl_SearchInitiated;
            pdfViewerControl.DoubleTapped += PdfViewerControl_DoubleTapped;
            pdfViewerControl.StampAnnotationSelected += PdfViewerControl_StampAnnotationSelected;
            pdfViewerControl.StampAnnotationDeselected += PdfViewerControl_StampAnnotationDeselected;

            pdfViewerControl.StampAnnotationAdded += PdfViewerControl_StampAnnotationAdded;


            RedoButton.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(async () =>
                {
                    if (CanRedoInk == true)
                    {
                        RedoInk();
                        await Task.Delay(100);

                        if (CanRedoInk == true)
                            RedoButton.Foreground = Color.FromHex("#373737");
                        else
                            RedoButton.Foreground = Color.Transparent;
                    }
                })
            });

            UndoButton.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(async () =>
                {
                    if (CanUndoInk == true)
                    {
                        UndoInk();
                        await Task.Delay(100);

                        if (CanUndoInk == true)
                            UndoButton.Foreground = Color.FromHex("#373737");
                        else
                            UndoButton.Foreground = Color.Transparent;
                    }
                })
            });

            ValidButton.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(async () =>
                {
                    if (CanUndoInk == true)
                    {
                        ValidButton.Foreground = Color.Transparent;
                        RedoButton.Foreground = Color.Transparent;
                        UndoButton.Foreground = Color.Transparent;

                        if (pdfViewerControl.AnnotationMode == AnnotationMode.Ink)
                            SaveInk();

                        pdfViewerControl.AnnotationMode = AnnotationMode.None;

                        await Task.Delay(100);

                        CanRedoInk = false;
                        CanUndoInk = false;
                    }

                    if (ShapeAnnotationsSelected() || TextMarkupAnnotationsSelected())
                    {
                        pdfViewerControl.AnnotationMode = AnnotationMode.None;

                        ValidButton.Foreground = Color.Transparent;
                    }
                })
            });

            paletteButton.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(PaletteButton_Clicked)
            });

            trashButton.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(TrashButton_Clicked)
            });
        }

        private bool TextMarkupAnnotationsSelected()
        {
            return pdfViewerControl.AnnotationMode == AnnotationMode.Strikethrough
                   || pdfViewerControl.AnnotationMode == AnnotationMode.Highlight
                   || pdfViewerControl.AnnotationMode == AnnotationMode.Underline;
        }

        private bool ShapeAnnotationsSelected()
        {
            return pdfViewerControl.AnnotationMode == AnnotationMode.Arrow
                   || pdfViewerControl.AnnotationMode == AnnotationMode.Line
                   || pdfViewerControl.AnnotationMode == AnnotationMode.Circle
                   || pdfViewerControl.AnnotationMode == AnnotationMode.Rectangle;
        }

        #endregion

        protected override bool OnBackButtonPressed()
        {
            DependencyService.Get<IToastMessage>().ShortAlert("tesdt");

            return true;

            // true or false to disable or enable the action

            //return base.OnBackButtonPressed();
        }


        #region Top Toolbar Event Handlers 
        private void BookmarkButton_Clicked(object sender, EventArgs e)
        {
            pdfViewerControl.BookmarkPaneVisible = true;
        }

        #region PopupMenu Methods

        private void CloseButtonPopupMenu_Clicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                popupMenu.IsOpen = false;
            });
        }

        private async void MenuListView_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            var itemMenu = (ItemsMenu)popupMenuContent.MenuListView.SelectedItem;
            popupMenuContent.MenuListView.SelectedItem = null;

            switch (itemMenu.Id)
            {
                case 0:
                    if (CanSaveDocument == true)
                        await SaveDocument();
                    break;
                case 1:
                    await PrintDocument();
                    break;
                default:
                    break;
            }
        }

        private async Task ShowThumnbailsView()
        {

        }

        private async Task PrintDocument()
        {
            UserDialogs.Instance.ShowLoading("Loading ...", MaskType.Black);

            await Task.Run(() =>
            {
                var fileName = Path.GetFileName(this.filePath);
                pdfViewerControl.Print(fileName);
            });

            Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.HideLoading();
                popupMenu.IsVisible = false;
            });
        }

        private async Task SaveDocument()
        {
            Dictionary<bool, string> saveStatus = null;

            Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.ShowLoading("Loading...", MaskType.Black);

                NumberOfAnnotation = 0;
            });

            await Task.Run(async () =>
            {
                using (Stream stream = await pdfViewerControl.SaveDocumentAsync())
                {
                    if (Device.RuntimePlatform == Device.Android)
                        saveStatus = await DependencyService.Get<IAndroidFileHelper>().Save(stream as MemoryStream, this.filePath);
                }
            });

            Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.HideLoading();

                if (saveStatus.ContainsKey(true) == true)
                {
                    if (Device.RuntimePlatform == Device.Android)
                        DependencyService.Get<IToastMessage>().LongAlert("Document saved");

                    popupMenu.IsOpen = false;
                }
                else
                {
                    if (Device.RuntimePlatform == Device.Android)
                        DependencyService.Get<IToastMessage>().LongAlert(saveStatus[false]);
                }
            });
        }

        private void MoreOptionButton_Clicked(object sender, EventArgs e)
        {
            popupMenu.Show();

            popupMenuContent.MenuListView.SelectionChanged += MenuListView_SelectionChanged;
            popupMenu.Closed += PopupMenu_Closed;
            image.Clicked += CloseButtonPopupMenu_Clicked;
        }

        private void PopupMenu_Closed(object sender, EventArgs e)
        {
            popupMenuContent.MenuListView.SelectionChanged -= MenuListView_SelectionChanged;
            popupMenu.Closed -= PopupMenu_Closed;
            image.Clicked -= CloseButtonPopupMenu_Clicked;
        }
        #endregion
        #endregion

        #region Pdf viewer events methods

        #region PDF Password Event Handlers
        private void AcceptButtonPasswordPopup_Clicked(object sender, EventArgs e)
        {
            UserDialogs.Instance.ShowLoading("Loading", MaskType.Black);

            passwordPopup.IsOpen = false;
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();

            isATryToOpenTheDocument = true;
        }

        private void PasswordPopup_Closed(object sender, EventArgs e)
        {
            if (isATryToOpenTheDocument == true)
            {
                pdfViewerControl.LoadDocument(pdfStream, entry.Text);
                isATryToOpenTheDocument = false;
            }
        }

        private async void DeclineButton_Clicked(object sender, EventArgs e)
        {
            passwordPopup.IsOpen = false;
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();

            await Navigation.PopAsync();
        }

        private void PdfViewerControl_PasswordErrorOccurred(object sender, PasswordErrorOccurredEventArgs e)
        {
            if (isPasswordPopupInitiInitalized == false)
            {
                passwordPopup = new SfPopupLayout
                {
                    PopupView =
                    {
                        HeightRequest = 200,
                        WidthRequest = 310,
                        ShowHeader = true,
                        ShowFooter = true,
                        ShowCloseButton = false,
                        AnimationDuration = 170,
                        HeaderHeight = 63
                    }
                };
                passwordPopup.Closed += PasswordPopup_Closed;
                passwordPopup.StaysOpen = true;

                DataTemplate templateViewHeaderSetPasswordPoop = new DataTemplate(() =>
                {
                    Label label = new Label
                    {
                        VerticalTextAlignment = Xamarin.Forms.TextAlignment.Center,
                        Padding = new Thickness(20, 0, 20, 0),
                        Text = "Password",
                        TextColor = Color.Black,
                        FontSize = 17,
                        FontFamily = "GothamMedium_1.ttf#GothamMedium_1"
                    };

                    return label;
                });


                templateViewSetPasswordPopup = new DataTemplate(() =>
                {
                    StackLayout stackLayout = new StackLayout();

                    label = new Label
                    {
                        Text = "This file is password protected. Please enter the password",
                        TextColor = Color.Black,
                        FontSize = 13.5,
                        Padding = new Thickness(20, 0, 20, 0),
                        FontFamily = "GothamMedium_1.ttf#GothamMedium_1"
                    };


                    entry = new Entry
                    {
                        FontSize = 13.5,
                        Margin = new Thickness(19, 0, 19, 0),
                        IsPassword = true,
                        Placeholder = "Enter the password",
                        TextColor = Color.Black
                    };

                    stackLayout.Children.Add(label);
                    stackLayout.Children.Add(entry);

                    return stackLayout;
                });

                DataTemplate footerTemplateViewSetPasswordPopup = new DataTemplate(() =>
                {
                    StackLayout stackLayout = new StackLayout {Orientation = StackOrientation.Horizontal, Spacing = 0};

                    acceptButton = new Button
                    {
                        Text = "Ok",
                        FontFamily = "GothamMedium_1.ttf#GothamMedium_1",
                        FontSize = 14,
                        TextColor = Color.Black,
                        HorizontalOptions = LayoutOptions.EndAndExpand,
                        BackgroundColor = Color.White,
                        VerticalOptions = LayoutOptions.Center,
                        WidthRequest = 86
                    };
                    acceptButton.Clicked += AcceptButtonPasswordPopup_Clicked;
                    declineButton = new Button
                    {
                        Text = "Cancel",
                        FontFamily = "GothamMedium_1.ttf#GothamMedium_1",
                        FontSize = 14,
                        TextColor = Color.Black,
                        HorizontalOptions = LayoutOptions.End,
                        BackgroundColor = Color.White,
                        VerticalOptions = LayoutOptions.Center,
                        WidthRequest = 89
                    };
                    declineButton.Clicked += DeclineButton_Clicked;

                    stackLayout.Children.Add(acceptButton);
                    stackLayout.Children.Add(declineButton);

                    return stackLayout;
                });

                // Adding ContentTemplate of the SfPopupLayout
                passwordPopup.PopupView.ContentTemplate = templateViewSetPasswordPopup;

                // Adding FooterTemplate of the SfPopupLayout
                passwordPopup.PopupView.FooterTemplate = footerTemplateViewSetPasswordPopup;

                // Adding FooterTemplate of the SfPopupLayout
                passwordPopup.PopupView.HeaderTemplate = templateViewHeaderSetPasswordPoop;

                isPasswordPopupInitiInitalized = true;

                Shell.SetNavBarIsVisible(this, false);
                Shell.SetTabBarIsVisible(this, false);
                Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
                NavigationPage.SetHasNavigationBar(this, false);
            }

            if (hasAlreadyTriedToOpenDoc == true)
            {
                passwordPopup.Show();
                label.Text = "The password is incorrect. Please try again";
                entry.Focus();
            }
            else
            {
                passwordPopup.Show();
                hasAlreadyTriedToOpenDoc = true;
            }
        }
        #endregion

        private void UpdateImages()
        {
            if (ThumbnailBytes.Count != 0)
            {
                int i = 0;
                foreach (var thumbnail in ThumbnailBytes)
                {
                    i++;

                    ThumbnailInfoCollection.Add(thumbnail);
                }
            }
        }

        private void ConvertStreamToImageSource(Stream[] imageStreams)
        {
            foreach (Stream imageStream in imageStreams)
            {
                imageStream.Position = 0;
                byte[] bytes = ReadBytes(imageStream).Result;
                ThumbnailBytes.Add(bytes);
            }
        }

        private async Task<byte[]> ReadBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await ms.WriteAsync(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private async void PdfViewerControl_DoubleTapped(object sender, TouchInteractionEventArgs e)
        {
            if (this.toolbarIsCollapsed == true)
            {
                pdfViewerControl.AnnotationSettings.IsLocked = false;

                if (Device.RuntimePlatform == Device.Android)
                    DependencyService.Get<INavBarHelper>().SetDefaultNavBar();

                await RevealToolbar();
            }
            else
            {
                pdfViewerControl.AnnotationSettings.IsLocked = true;

                if (Device.RuntimePlatform == Device.Android)
                    DependencyService.Get<INavBarHelper>().SetImmersiveMode();

                await CollapseToolbar();
            }
        }

        private async Task CollapseToolbar()
        {
            pdfViewGrid.Margin = 0;

            await topToolbar.FadeTo(0, 150);
            await bottomMainBar.FadeTo(0, 150);

            toolbarIsCollapsed = true;
        }

        private async Task RevealToolbar()
        {
            await topToolbar.FadeTo(1, 150);
            await bottomMainBar.FadeTo(1, 150);

            await Task.Delay(150);

            pdfViewGrid.Margin = new Thickness(0, 45, 0, 45);

            toolbarIsCollapsed = false;
        }

        private void PdfViewerControl_PageChanged(object sender, PageChangedEventArgs args)
        {
            switch (annotationType)
            {
                case AnnotationType.Ink:
                    pdfViewerControl.AnnotationMode = AnnotationMode.Ink;
                    break;
                case AnnotationType.Rectangle:
                    pdfViewerControl.AnnotationMode = AnnotationMode.Rectangle;
                    break;
                case AnnotationType.Line:
                    pdfViewerControl.AnnotationMode = AnnotationMode.Line;
                    break;
                case AnnotationType.Arrow:
                    pdfViewerControl.AnnotationMode = AnnotationMode.Arrow;
                    break;
                case AnnotationType.Circle:
                    pdfViewerControl.AnnotationMode = AnnotationMode.Circle;
                    break;
                case AnnotationType.Hightlight:
                    pdfViewerControl.AnnotationMode = AnnotationMode.Highlight;
                    break;
                case AnnotationType.Underline:
                    pdfViewerControl.AnnotationMode = AnnotationMode.Underline;
                    break;
                case AnnotationType.Strikethrought:
                    pdfViewerControl.AnnotationMode = AnnotationMode.Strikethrough;
                    break;
                case AnnotationType.None:
                    break;
                default:
                    break;
            }
        }

        #region Annotation Selected Event Handlers

        private void PdfViewerControl_StampAnnotationDeselected(object sender, StampAnnotationDeselectedEventArgs e)
        {
            BackButtonAnnotationTypeToolbar_Clicked(null, null);
        }

        private void PdfViewerControl_StampAnnotationSelected(object sender, StampAnnotationSelectedEventArgs e)
        {
            SetToolbarForStampAnnotationSelected();

            selectedStampAnnotation = sender as StampAnnotation;

            this.annotationType = AnnotationType.Stamp;
        }

        private void PdfViewerControl_TextMarkupDeselected(object sender, TextMarkupDeselectedEventArgs args)
        {
            IsTextMarkupMenuEnabled = false;

            selectedShapeAnnotation = null;
        }

        private void PdfViewerControl_TextMarkupSelected(object sender, TextMarkupSelectedEventArgs args)
        {
            //Cast the sender object to FreeTextAnnotation
            selectedTextMarkupAnnotation = sender as TextMarkupAnnotation;

            SelectedColor = selectedTextMarkupAnnotation.Settings.Color;

            switch (args.TextMarkupAnnotationType)
            {
                case TextMarkupAnnotationType.Strikethrough:
                    this.annotationType = AnnotationType.Strikethrought;
                    break;
                case TextMarkupAnnotationType.Underline:
                    this.annotationType = AnnotationType.Underline;
                    break;
                case TextMarkupAnnotationType.Highlight:
                    this.annotationType = AnnotationType.Hightlight;
                    break;
                default:
                    break;
            }

            IsTextMarkupMenuEnabled = true;
        }

        private void PdfViewerControl_ShapeAnnotationDeselected(object sender, ShapeAnnotationDeselectedEventArgs args)
        {
            IsShapeMenuEnabled = false;

            selectedShapeAnnotation = null;
        }

        private void PdfViewerControl_ShapeAnnotationSelected(object sender, ShapeAnnotationSelectedEventArgs args)
        {
            //Cast the sender object to FreeTextAnnotation
            this.selectedShapeAnnotation = sender as ShapeAnnotation;

            SelectedColor = this.selectedShapeAnnotation.Settings.StrokeColor;

            switch (args.AnnotationType)
            {
                case AnnotationMode.Rectangle:
                    annotationType = AnnotationType.Rectangle;
                    break;
                case AnnotationMode.Circle:
                    annotationType = AnnotationType.Circle;
                    break;
                case AnnotationMode.Line:
                    annotationType = AnnotationType.Line;
                    break;
                case AnnotationMode.Arrow:
                    annotationType = AnnotationType.Arrow;
                    break;
                default:
                    break;
            }

            IsShapeMenuEnabled = true;
        }

        private void PdfViewerControl_FreeTextAnnotationDeselected(object sender, FreeTextAnnotationDeselectedEventArgs args)
        {
            IsFreeTextMenuEnabled = false;

            selectedFreeTextAnnotation = null;
        }

        private void PdfViewerControl_FreeTextAnnotationSelected(object sender, FreeTextAnnotationSelectedEventArgs args)
        {
            this.annotationType = AnnotationType.FreeText;

            this.selectedFreeTextAnnotation = sender as FreeTextAnnotation;

            SelectedColor = args.TextColor;

            IsFreeTextMenuEnabled = true;
        }

        private void PdfViewerControl_InkSelected(object sender, InkSelectedEventArgs args)
        {
            if (args.IsSignature)
            {
                this.annotationType = AnnotationType.HandwrittenSignature;
                selectedHandwrittenSignature = sender as HandwrittenSignature;

                IsHandwrittenSignatureMenuEnabled = true;
            }
            else
            {
                this.annotationType = AnnotationType.Ink;
                selectedInkAnnotation = sender as InkAnnotation;

                SelectedColor = args.Color;

                IsInkMenuEnabled = true;
            }

        }

        private void PdfViewerControl_InkDeselected(object sender, InkDeselectedEventArgs args)
        {
            IsInkMenuEnabled = false;
            IsHandwrittenSignatureMenuEnabled = false;

            selectedInkAnnotation = null;
            selectedHandwrittenSignature = null;
        }
        #endregion

        #region Annotation Added Events Handler 
        private void PdfViewerControl_FreeTextAnnotationAdded(object sender, FreeTextAnnotationAddedEventArgs args)
        {
            NumberOfAnnotation += 1;

            BackButtonAnnotationTypeToolbar_Clicked(null, null);
        }

        private void PdfViewerControl_ShapeAnnotationAdded(object sender, ShapeAnnotationAddedEventArgs args)
        {
            NumberOfAnnotation += 1;

            this.ShapesNumbers += 1;
        }

        private void PdfViewerControl_StampAnnotationAdded(object sender, StampAnnotationAddedEventArgs e)
        {
            NumberOfAnnotation += 1;
        }
        private void PdfViewerControl_InkAdded(object sender, InkAddedEventArgs args)
        {
            NumberOfAnnotation += 1;
        }

        private void PdfViewerControl_TextMarkupAdded(object sender, TextMarkupAddedEventArgs args)
        {
            NumberOfAnnotation += 1;

            this.TextMarkupNumbers += 1;
        }
        #endregion

        #region Perform ink annotations
        private void PdfViewerControl_CanUndoInkModified(object sender, CanUndoInkModifiedEventArgs args)
        {
            CanUndoInk = args.CanUndoInk;
        }

        private void PdfViewerControl_CanRedoInkModified(object sender, CanRedoInkModifiedEventArgs args)
        {
            CanRedoInk = args.CanRedoInk;
        }

        private void UndoInk()
        {
            if (CanUndoInk == true)
                pdfViewerControl.UndoInk();
        }

        private void RedoInk()
        {
            if (CanRedoInk == true)
                pdfViewerControl.RedoInk();
        }

        private async void SaveInk()
        {
            pdfViewerControl.EndInkSession(true);

            await Task.Run(async () =>
            {
                await annotationTypeToolbar.LayoutTo(new Rectangle(annotationTypeToolbar.Bounds.X, annotationTypeToolbar.Bounds.Y, annotationTypeToolbar.Bounds.Width, 0), 200, Easing.Linear);
            });

            CollapseAnnotationsToolbar();
        }

        private void CollapseAnnotationsToolbar()
        {
            annotationTypeToolbar.IsVisible = false;

            pdfViewerControl.AnnotationMode = AnnotationMode.None;
            annotationType = AnnotationType.None;
            bottomMainToolbar.IsVisible = true;

            viewModeButton.IsVisible = true;
            bookmarkButton.IsVisible = true;
            searchButton.IsVisible = true;
            moreOptionButton.IsVisible = true;

            ValidButton.IsVisible = false;
            UndoButton.IsVisible = false;
            RedoButton.IsVisible = false;
        }

        #endregion

        #endregion

        #region ThicknessBarEvents
        private void ThicknessBar_Clicked(int numberOfThicknessBarClicked)
        {
            if (lastThicknessBarSelected != numberOfThicknessBarClicked)
            {
                switch (lastThicknessBarSelected)
                {
                    case 1:
                        styleContent.ThicknessBar.FirstThicknessBar.BorderThickness = 0;
                        break;
                    case 2:
                        styleContent.ThicknessBar.SecondThicknessBar.BorderThickness = 0;
                        break;
                    case 3:
                        styleContent.ThicknessBar.ThirdThicknessBar.BorderThickness = 0;
                        break;
                    case 4:
                        styleContent.ThicknessBar.FourthThicknessBar.BorderThickness = 0;
                        break;
                    case 5:
                        styleContent.ThicknessBar.FifthThicknessBar.BorderThickness = 0;
                        break;
                    default:
                        break;
                }

                switch (numberOfThicknessBarClicked)
                {
                    case 1:
                        styleContent.ThicknessBar.FirstThicknessBar.BorderThickness = 1;
                        ChangeThicknessForAnnotation(2);
                        break;
                    case 2:
                        styleContent.ThicknessBar.SecondThicknessBar.BorderThickness = 2;
                        ChangeThicknessForAnnotation(4);
                        break;
                    case 3:
                        styleContent.ThicknessBar.ThirdThicknessBar.BorderThickness = 2;
                        ChangeThicknessForAnnotation(6);
                        break;
                    case 4:
                        styleContent.ThicknessBar.FourthThicknessBar.BorderThickness = 2;
                        ChangeThicknessForAnnotation(8);
                        break;
                    case 5:
                        styleContent.ThicknessBar.FifthThicknessBar.BorderThickness = 2;
                        ChangeThicknessForAnnotation(9);
                        break;
                    default:
                        break;
                }
            }

            lastThicknessBarSelected = numberOfThicknessBarClicked;
        }

        private void ChangeThicknessForAnnotation(int thicknessValue)
        {
            if (selectedInkAnnotation != null)
                selectedInkAnnotation.Settings.Thickness = thicknessValue;
            else
            {
                if (selectedShapeAnnotation != null)
                    selectedShapeAnnotation.Settings.Thickness = thicknessValue;
                else
                {
                    switch (pdfViewerControl.AnnotationMode)
                    {
                        case AnnotationMode.Ink:
                            pdfViewerControl.AnnotationSettings.Ink.Thickness = thicknessValue;
                            break;
                        case AnnotationMode.Rectangle:
                            pdfViewerControl.AnnotationSettings.Rectangle.Settings.Thickness = thicknessValue;
                            break;
                        case AnnotationMode.Circle:
                            pdfViewerControl.AnnotationSettings.Circle.Settings.Thickness = thicknessValue;
                            break;
                        case AnnotationMode.Line:
                            pdfViewerControl.AnnotationSettings.Line.Settings.Thickness = thicknessValue;
                            break;
                        case AnnotationMode.Arrow:
                            pdfViewerControl.AnnotationSettings.Arrow.Settings.Thickness = thicknessValue;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        #endregion

        #region Opacity Methods 
        private void OpacityIcon_Clicked(int numberOfTheOpacitySelected)
        {
            if (lastOpacitySelected != numberOfTheOpacitySelected)
            {
                switch (lastOpacitySelected)
                {
                    case 1:
                        styleContent.OpacityImageTo25.Source = "baseline_invert_colors_off_24.xml";
                        break;
                    case 2:
                        styleContent.OpacityImageTo50.Source = "baseline_invert_colors_off_24.xml";
                        break;
                    case 3:
                        styleContent.OpacityImageTo75.Source = "baseline_invert_colors_off_24.xml";
                        break;
                    case 4:
                        styleContent.OpacityImageTo100.Source = "baseline_invert_colors_off_24.xml";
                        break;
                    default:
                        break;
                }

                switch (numberOfTheOpacitySelected)
                {
                    case 1:
                        styleContent.OpacityImageTo25.Source = "baseline_opacity_24.xml";
                        ChangeOpacityValue(0.25f);
                        break;
                    case 2:
                        styleContent.OpacityImageTo50.Source = "baseline_opacity_24.xml";
                        ChangeOpacityValue(0.50f);
                        break;
                    case 3:
                        styleContent.OpacityImageTo75.Source = "baseline_opacity_24.xml";
                        ChangeOpacityValue(0.75f);
                        break;
                    case 4:
                        styleContent.OpacityImageTo100.Source = "baseline_opacity_24.xml";
                        ChangeOpacityValue(1f);
                        break;
                    default:
                        break;
                }
            }


            lastOpacitySelected = numberOfTheOpacitySelected;
        }

        private void ChangeOpacityValue(float opacityValue)
        {
            if (selectedInkAnnotation != null)
                selectedInkAnnotation.Settings.Opacity = opacityValue;
            else
            {
                if (selectedShapeAnnotation != null)
                    selectedShapeAnnotation.Settings.Opacity = opacityValue;
                else
                {
                    switch (pdfViewerControl.AnnotationMode)
                    {
                        case AnnotationMode.Ink:
                            pdfViewerControl.AnnotationSettings.Ink.Opacity = opacityValue;
                            break;
                        case AnnotationMode.Rectangle:
                            pdfViewerControl.AnnotationSettings.Rectangle.Settings.Opacity = opacityValue;
                            break;
                        case AnnotationMode.Circle:
                            pdfViewerControl.AnnotationSettings.Circle.Settings.Opacity = opacityValue;
                            break;
                        case AnnotationMode.Line:
                            pdfViewerControl.AnnotationSettings.Line.Settings.Opacity = opacityValue;
                            break;
                        case AnnotationMode.Arrow:
                            pdfViewerControl.AnnotationSettings.Arrow.Settings.Opacity = opacityValue;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        #endregion

        #region BottomMainBarMethods
        private async void SignatureButton_Clicked(object sender, EventArgs e)
        {
            pdfViewerControl.AnnotationMode = AnnotationMode.HandwrittenSignature;
        }

        private void StampListView_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            var stampItem = (StampModel)stampSlideUpMenu.StampListView.SelectedItem;

            //Set image source
            Image image = new Image {Source = stampItem.Image, WidthRequest = 350, HeightRequest = 250};

            //Add image as custom stamp to the first page
            pdfViewerControl.AddStamp(image, pdfViewerControl.PageNumber);

            this.HideMenu();
        }

        private void StampButton_Clicked(object sender, EventArgs e)
        {
            this.ShowMenu();
        }

        private async void TextMarkupButton_Clicked(object sender, EventArgs e)
        {
            await CollapseBottomMainToolbar(AnnotationType.TextMarkup);
        }

        private async void ShapeButton_Clicked(object sender, EventArgs e)
        {
            await CollapseBottomMainToolbar(AnnotationType.Shape);
        }

        private async void InkButton_Clicked(object sender, EventArgs e)
        {
            await CollapseBottomMainToolbar(AnnotationType.Ink);
        }

        private async void FreeTextButton_Clicked(object sender, EventArgs e)
        {
            await CollapseBottomMainToolbar(AnnotationType.FreeText);
        }

        #region Enable Annotation toolbar Methods 
        private void SetToolbarForShapeAnnotationSelected()
        {
            bottomMainToolbar.IsVisible = false;
            textMarkupToolbar.IsVisible = false;

            styleContent.FontSizeControl.IsVisible = false;

            stylePopup.PopupView.HeightRequest = 195;
            stylePopup.PopupView.WidthRequest = 280;

            shapeToolbar.IsVisible = true;
            styleContent.BoxView2.IsVisible = true;
            styleContent.BoxView1.IsVisible = true;
            styleContent.OpacityControl.IsVisible = true;
            styleContent.ThicknessBar.IsVisible = true;
        }

        private void SetToolbarForFreeTextAnnotationSelected()
        {
            bottomMainToolbar.IsVisible = false;
            shapeToolbar.IsVisible = false;
            textMarkupToolbar.IsVisible = false;

            styleContent.OpacityControl.IsVisible = false;
            styleContent.BoxView2.IsVisible = false;
            styleContent.ThicknessBar.IsVisible = false;

            stylePopup.PopupView.HeightRequest = 130;
            stylePopup.PopupView.WidthRequest = 280;

            styleContent.FontSizeControl.IsVisible = true;
            styleContent.BoxView1.IsVisible = true;
            paletteButton.IsVisible = true;
            annotationTypeToolbar.IsVisible = true;

            imageAnnotationType.Source = "twotone_title_24.xml";

            if (this.annotationType != AnnotationType.FreeText)
            {
                this.annotationType = AnnotationType.FreeText;
                pdfViewerControl.AnnotationMode = AnnotationMode.FreeText;
                pdfViewerControl.AnnotationSettings.FreeText.TextSize = 8;
                this.FontSize = 8;
                styleContent.ColorPicker.SelectedIndex = 0;
            }
            else
            {
                trashButton.IsVisible = true;
            }
        }

        private void SetToolbarForInkAnnotationSelected()
        {
            bottomMainToolbar.IsVisible = false;
            shapeToolbar.IsVisible = false;
            textMarkupToolbar.IsVisible = false;
            styleContent.FontSizeControl.IsVisible = false;
            viewModeButton.IsVisible = false;
            bookmarkButton.IsVisible = false;
            searchButton.IsVisible = false;
            moreOptionButton.IsVisible = false;

            imageAnnotationType.Source = "pen.png";

            paletteButton.IsVisible = true;
            annotationTypeToolbar.IsVisible = true;
            styleContent.OpacityControl.IsVisible = true;
            styleContent.ThicknessBar.IsVisible = true;
            styleContent.BoxView2.IsVisible = true;
            styleContent.BoxView1.IsVisible = true;

            ValidButton.IsVisible = true;
            UndoButton.IsVisible = true;
            RedoButton.IsVisible = true;

            stylePopup.PopupView.HeightRequest = 195;
            stylePopup.PopupView.WidthRequest = 280;

            if (this.annotationType != AnnotationType.Ink)
            {
                this.annotationType = AnnotationType.Ink;
                pdfViewerControl.AnnotationMode = AnnotationMode.Ink;

                pdfViewerControl.AnnotationSettings.Ink.Thickness = 9;
                styleContent.ColorPicker.SelectedIndex = 0;
            }
            else
            {
                trashButton.IsVisible = true;
            }
        }

        private void SetToolbarForTextMarkupAnnotationSelected()
        {
            bottomMainToolbar.IsVisible = false;
            shapeToolbar.IsVisible = false;

            textMarkupToolbar.IsVisible = true;

            styleContent.OpacityControl.IsVisible = false;
            styleContent.ThicknessBar.IsVisible = false;
            styleContent.BoxView2.IsVisible = false;
            styleContent.BoxView1.IsVisible = false;

            stylePopup.PopupView.HeightRequest = 62;
            stylePopup.PopupView.WidthRequest = 280;
        }

        private void SetToolbarForStampAnnotationSelected()
        {
            bottomMainToolbar.IsVisible = false;
            shapeToolbar.IsVisible = false;
            textMarkupToolbar.IsVisible = false;

            paletteButton.IsVisible = false;
            annotationTypeToolbar.IsVisible = true;

            imageAnnotationType.Source = "stamp.png";

            trashButton.IsVisible = true;
        }

        private void SetToolbarForHandwrittenSignatureSelected()
        {
            bottomMainToolbar.IsVisible = false;
            shapeToolbar.IsVisible = false;
            textMarkupToolbar.IsVisible = false;

            paletteButton.IsVisible = false;
            annotationTypeToolbar.IsVisible = true;

            imageAnnotationType.Source = "signature.png";

            trashButton.IsVisible = true;
        }
        #endregion

        private async Task CollapseBottomMainToolbar(AnnotationType annotationType)
        {
            switch (annotationType)
            {
                case AnnotationType.Ink:
                    IsInkMenuEnabled = true;
                    break;
                case AnnotationType.FreeText:
                    IsFreeTextMenuEnabled = true;
                    break;
                case AnnotationType.Shape:
                    SetToolbarForShapeAnnotationSelected();
                    break;
                case AnnotationType.TextMarkup:
                    SetToolbarForTextMarkupAnnotationSelected();
                    break;
                case AnnotationType.None:
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region TextMarkup toolbar methods
        private async void BackButtonTextMarkupToolbar_Clicked(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                await textMarkupToolbar.LayoutTo(new Rectangle(textMarkupToolbar.Bounds.X, textMarkupToolbar.Bounds.Y, textMarkupToolbar.Bounds.Width, 0), 200, Easing.Linear);
            });

            imageAnnotationType.HeightRequest = 29;
            imageAnnotationType.WidthRequest = 29;
            textMarkupToolbar.IsVisible = false;
            bottomMainToolbar.IsVisible = true;
        }

        private void StrikethroughtButton_Clicked(object sender, EventArgs e)
        {
            bottomMainToolbar.IsVisible = false;

            imageAnnotationType.Source = "ic_strikethrough.png";

            imageAnnotationType.HeightRequest = 30;
            imageAnnotationType.WidthRequest = 30;

            if (this.annotationType != AnnotationType.Strikethrought)
            {
                pdfViewerControl.AnnotationMode = AnnotationMode.Strikethrough;
                this.annotationType = AnnotationType.Strikethrought;

                styleContent.ColorPicker.SelectedIndex = 5;
            }
            else
            {
                trashButton.IsVisible = true;
            }

            InitializeComponentForTextMarkupAnnotation();
        }

        private void UnderlineButton_Clicked(object sender, EventArgs e)
        {
            bottomMainToolbar.IsVisible = false;

            imageAnnotationType.Source = "ic_underline.png";
            imageAnnotationType.HeightRequest = 25;
            imageAnnotationType.WidthRequest = 25;

            if (this.annotationType != AnnotationType.Underline)
            {
                pdfViewerControl.AnnotationMode = AnnotationMode.Underline;
                this.annotationType = AnnotationType.Underline;
                //todo -- set for red color
                styleContent.ColorPicker.SelectedIndex = 4;
            }
            else
            {
                trashButton.IsVisible = true;
            }

            InitializeComponentForTextMarkupAnnotation();
        }

        private void HightlightButton_Clicked(object sender, EventArgs e)
        {
            bottomMainToolbar.IsVisible = false;

            imageAnnotationType.Source = "ic_edit.png";
            imageAnnotationType.HeightRequest = 25;
            imageAnnotationType.WidthRequest = 25;

            if (this.annotationType != AnnotationType.Hightlight)
            {
                pdfViewerControl.AnnotationMode = AnnotationMode.Highlight;
                this.annotationType = AnnotationType.Hightlight;
                styleContent.ColorPicker.SelectedIndex = 5;
            }
            else
            {
                trashButton.IsVisible = true;
            }

            InitializeComponentForTextMarkupAnnotation();
        }

        private void InitializeComponentForTextMarkupAnnotation()
        {
            paletteButton.IsVisible = true;
            textMarkupToolbar.IsVisible = false;

            annotationTypeToolbar.IsVisible = true;
        }
        #endregion

        #region AnnotationType toolbar methods
        private async void BackButtonAnnotationTypeToolbar_Clicked(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                await annotationTypeToolbar.LayoutTo(new Rectangle(annotationTypeToolbar.Bounds.X, annotationTypeToolbar.Bounds.Y, annotationTypeToolbar.Bounds.Width, 0), 200, Easing.Linear);
            });

            annotationTypeToolbar.IsVisible = false;

            switch (this.annotationType)
            {
                case AnnotationType.Ink:
                    pdfViewerControl.SelectionMode = Syncfusion.SfPdfViewer.XForms.SelectionMode.None;
                    annotationType = AnnotationType.None;
                    bottomMainToolbar.IsVisible = true;

                    viewModeButton.IsVisible = true;
                    bookmarkButton.IsVisible = true;
                    searchButton.IsVisible = true;
                    moreOptionButton.IsVisible = true;

                    ValidButton.IsVisible = false;
                    UndoButton.IsVisible = false;
                    RedoButton.IsVisible = false;
                    break;
                case AnnotationType.HandwrittenSignature:
                    pdfViewerControl.SelectionMode = Syncfusion.SfPdfViewer.XForms.SelectionMode.None;
                    annotationType = AnnotationType.None;
                    bottomMainToolbar.IsVisible = true;
                    break;
                case AnnotationType.FreeText:
                    pdfViewerControl.SelectionMode = Syncfusion.SfPdfViewer.XForms.SelectionMode.None;
                    annotationType = AnnotationType.None;
                    bottomMainToolbar.IsVisible = true;
                    break;
                case (AnnotationType.Arrow):
                    shapeToolbar.IsVisible = true;
                    pdfViewerControl.AnnotationMode = AnnotationMode.None;
                    pdfViewerControl.SelectionMode = Syncfusion.SfPdfViewer.XForms.SelectionMode.None;
                    annotationType = AnnotationType.None;
                    break;
                case (AnnotationType.Circle):
                    pdfViewerControl.AnnotationMode = AnnotationMode.None;
                    shapeToolbar.IsVisible = true;
                    pdfViewerControl.SelectionMode = Syncfusion.SfPdfViewer.XForms.SelectionMode.None;
                    annotationType = AnnotationType.None;
                    break;
                case (AnnotationType.Line):
                    pdfViewerControl.AnnotationMode = AnnotationMode.None;
                    shapeToolbar.IsVisible = true;
                    pdfViewerControl.SelectionMode = Syncfusion.SfPdfViewer.XForms.SelectionMode.None;
                    annotationType = AnnotationType.None;
                    break;
                case (AnnotationType.Rectangle):
                    pdfViewerControl.AnnotationMode = AnnotationMode.None;
                    shapeToolbar.IsVisible = true;
                    pdfViewerControl.SelectionMode = Syncfusion.SfPdfViewer.XForms.SelectionMode.None;
                    annotationType = AnnotationType.None;
                    break;
                case (AnnotationType.Hightlight):
                    textMarkupToolbar.IsVisible = true;
                    pdfViewerControl.SelectionMode = Syncfusion.SfPdfViewer.XForms.SelectionMode.None;
                    annotationType = AnnotationType.None;
                    break;
                case (AnnotationType.Strikethrought):
                    textMarkupToolbar.IsVisible = true;
                    pdfViewerControl.SelectionMode = Syncfusion.SfPdfViewer.XForms.SelectionMode.None;
                    annotationType = AnnotationType.None;
                    break;
                case (AnnotationType.Underline):
                    textMarkupToolbar.IsVisible = true;
                    pdfViewerControl.SelectionMode = Syncfusion.SfPdfViewer.XForms.SelectionMode.None;
                    annotationType = AnnotationType.None;
                    break;
                case (AnnotationType.Stamp):
                    pdfViewerControl.SelectionMode = Syncfusion.SfPdfViewer.XForms.SelectionMode.None;
                    annotationType = AnnotationType.None;
                    bottomMainToolbar.IsVisible = true;
                    break;
                case AnnotationType.None:
                    break;
                default:
                    break;
            }
        }

        private void PaletteButton_Clicked()
        {
            stylePopup.ShowRelativeToView(paletteButton, RelativePosition.AlignBottomRight, 150, 0);
        }

        private void TrashButton_Clicked()
        {
            if (selectedFreeTextAnnotation != null)
            {
                pdfViewerControl.RemoveAnnotation(selectedFreeTextAnnotation);
                selectedFreeTextAnnotation = null;
            }
            else
            {
                if (selectedInkAnnotation != null)
                {
                    pdfViewerControl.RemoveAnnotation(selectedInkAnnotation);
                    selectedInkAnnotation = null;
                }
                else
                {
                    if (selectedShapeAnnotation != null)
                    {
                        pdfViewerControl.RemoveAnnotation(selectedShapeAnnotation);

                        selectedShapeAnnotation = null;
                    }
                    else
                    {
                        if (selectedTextMarkupAnnotation != null)
                        {
                            pdfViewerControl.RemoveAnnotation(selectedTextMarkupAnnotation);

                            selectedTextMarkupAnnotation = null;
                        }
                        else
                        {
                            if (selectedStampAnnotation != null)
                            {
                                pdfViewerControl.RemoveAnnotation(selectedStampAnnotation);

                                selectedStampAnnotation = null;

                                BackButtonAnnotationTypeToolbar_Clicked(null, null);
                            }
                            else
                            {
                                if (selectedHandwrittenSignature != null)
                                {
                                    pdfViewerControl.RemoveAnnotation(selectedHandwrittenSignature);

                                    selectedHandwrittenSignature = null;

                                    BackButtonAnnotationTypeToolbar_Clicked(null, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Shape bar methods
        private async void BackButtonShapeToolbar_Clicked(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                await shapeToolbar.LayoutTo(new Rectangle(shapeToolbar.Bounds.X, shapeToolbar.Bounds.Y, shapeToolbar.Bounds.Width, 0), 200, Easing.Linear);
            });

            shapeToolbar.IsVisible = false;
            paletteButton.IsVisible = false;
            bottomMainToolbar.IsVisible = true;
        }

        private void CircleButton_Clicked(object sender, EventArgs e)
        {
            bottomMainToolbar.IsVisible = false;
            imageAnnotationType.HeightRequest = 32;
            imageAnnotationType.WidthRequest = 32;
            imageAnnotationType.Source = "ic_ui.png";

            if (this.annotationType != AnnotationType.Circle)
            {
                pdfViewerControl.AnnotationMode = AnnotationMode.Circle;
                this.annotationType = AnnotationType.Circle;
            }
            else
            {
                trashButton.IsVisible = true;
            }

            InitializeComponentsForShapeBar();
        }

        private void LineButton_Clicked(object sender, EventArgs e)
        {
            bottomMainToolbar.IsVisible = false;
            imageAnnotationType.HeightRequest = 28;
            imageAnnotationType.WidthRequest = 28;

            imageAnnotationType.Source = "ic_square.png";

            if (this.annotationType != AnnotationType.Line)
            {
                pdfViewerControl.AnnotationMode = AnnotationMode.Line;
                this.annotationType = AnnotationType.Line;
            }
            else
            {
                trashButton.IsVisible = true;
            }

            InitializeComponentsForShapeBar();
        }

        private void ArrowButton_Clicked(object sender, EventArgs e)
        {
            bottomMainToolbar.IsVisible = false;
            imageAnnotationType.HeightRequest = 30;
            imageAnnotationType.WidthRequest = 30;

            imageAnnotationType.Source = "ic_directional.png";

            if (this.annotationType != AnnotationType.Arrow)
            {
                pdfViewerControl.AnnotationMode = AnnotationMode.Arrow;
                this.annotationType = AnnotationType.Arrow;
            }
            else
            {
                trashButton.IsVisible = true;
            }

            InitializeComponentsForShapeBar();
        }

        private void RectangleButton_Clicked(object sender, EventArgs e)
        {
            bottomMainToolbar.IsVisible = false;
            imageAnnotationType.HeightRequest = 28;
            imageAnnotationType.WidthRequest = 28;

            imageAnnotationType.Source = "ic_math.png";

            if (this.annotationType != AnnotationType.Rectangle)
            {
                pdfViewerControl.AnnotationMode = AnnotationMode.Rectangle;
                this.annotationType = AnnotationType.Rectangle;
            }
            else
            {
                trashButton.IsVisible = true;
            }

            InitializeComponentsForShapeBar();
        }

        private void InitializeComponentsForShapeBar()
        {
            shapeToolbar.IsVisible = false;

            annotationTypeToolbar.IsVisible = true;
            paletteButton.IsVisible = true;

            styleContent.ColorPicker.SelectedIndex = 0;
            pdfViewerControl.AnnotationSettings.Rectangle.Settings.Thickness = 9;
        }
        #endregion

        #region Change view mode feature
        private void ViewModeButton_Clicked(object sender, EventArgs e)
        {
            if (pdfViewerControl.PageViewMode == PageViewMode.PageByPage)
            {
                pdfViewerControl.PageViewMode = PageViewMode.Continuous;
                viewModeButton.RotateTo(90);
            }
            else
            {
                pdfViewerControl.PageViewMode = PageViewMode.PageByPage;
                viewModeButton.RotateTo(180);
            }
        }
        #endregion

        #region Search text feature
        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            topMainBar.IsVisible = false;
            viewModeButton.IsVisible = false;

            searchBar.IsVisible = true;

        }

        private void CancelSearchButton_Clicked(object sender, EventArgs e)
        {
            PdfViewerControl.CancelSearch();

            topMainBar.IsVisible = true;
            viewModeButton.IsVisible = true;

            searchBar.IsVisible = false;
        }

        private void PdfViewerControl_SearchCompleted(object sender, TextSearchCompletedEventArgs args)
        {
            UserDialogs.Instance.HideLoading();

            if (args.NoMatchFound)
            {
                //Show popup
                searchErrorPopupContent.NoOccurenceFound.IsVisible = true;
                searchErrorPopupContent.NoMoreOccurenceFound.IsVisible = false;
                errorSearchPopup.Show();
            }
            else if (args.NoMoreOccurrence)
            {
                //Show popup
                searchErrorPopupContent.NoOccurenceFound.IsVisible = false;
                searchErrorPopupContent.NoMoreOccurenceFound.IsVisible = true;
                errorSearchPopup.Show();
            }

            searchStarted = false;
        }

        private void TextSearchEntry_Completed(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textSearchEntry.Text) && !string.IsNullOrEmpty(textSearchEntry.Text))
            {
                //Initiates text search.
                pdfViewerControl.SearchText(textSearchEntry.Text);
            }
            if (string.IsNullOrWhiteSpace(textSearchEntry.Text) || string.IsNullOrEmpty(textSearchEntry.Text))
            {
                pdfViewerControl.CancelSearch();
                searchStarted = false;
            }
            if (!searchStarted)
            {

                pdfViewerControl.SearchText(textSearchEntry.Text);
            }
            else
            {
                pdfViewerControl.SearchNext();
            }

            searchStarted = true;
        }

        private void SearchPreviousButton_Clicked(object sender, EventArgs e)
        {
            if (textSearchEntry.Text != string.Empty)
            {
                pdfViewerControl.SearchPreviousTextCommand.Execute(textSearchEntry.Text);
            }
        }

        private void SearchNextButton_Clicked(object sender, EventArgs e)
        {
            if (textSearchEntry.Text != string.Empty)
            {
                pdfViewerControl.SearchNextTextCommand.Execute(textSearchEntry.Text);
            }
        }

        private void PdfViewerControl_TextMatchFound(object sender, TextMatchFoundEventArgs args)
        {
            searchPreviousButton.IsVisible = true;
            searchNextButton.IsVisible = true;

            UserDialogs.Instance.HideLoading();
        }

        private void TextSearchEntry_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            pdfViewerControl.CancelSearch();
            searchStarted = false;

            searchPreviousButton.IsVisible = false;
            searchNextButton.IsVisible = false;
        }

        private void PdfViewerControl_SearchInitiated(object sender, TextSearchInitiatedEventArgs args)
        {
            UserDialogs.Instance.ShowLoading("Loading ...", MaskType.Clear);
        }
        #endregion

        #region On Property Changed
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}