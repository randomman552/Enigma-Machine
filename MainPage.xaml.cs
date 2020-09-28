using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Enigma_Machine
{
    public sealed partial class MainPage : Page
    {
        //TODO: Remove the heavy use of colors on the switch board to make the program more accessible

        /// <summary>
        /// Enigma object which actually handles encoding and decoding
        /// </summary>
        private Enigma enigma = new Enigma();


        /// <summary>
        /// This variable is where wheel states are saved so they can be loaded with the load button.
        /// </summary>
        private int[] WheelSave;


        /// <summary>
        /// This variable is where the switch board variables are stored between save/load events
        /// </summary>
        private Dictionary<char, char> SwitchBoardSave;


        /// <summary>
        /// The first switch board button pressed is store in this variable until the second one is pressed, they are then linked.
        /// </summary>
        private Button SwitchBoardButton = null;


        /// <summary>
        /// Dictionary to store a mapping of characters to the buttons that represnent them on the switch board
        /// </summary>
        private Dictionary<char, Button> sbBtns = new Dictionary<char, Button>();
        

        /// <summary>
        /// Colors used for the switch board buttons when linked.
        /// Need at least 13 so that all letters of the alphabet can be covered.
        /// </summary>
        private readonly Color[] sbBtnColors = new Color[] { 
            Colors.DarkRed,
            Colors.DarkMagenta,
            Colors.DarkViolet,
            Colors.Lime,
            Colors.LightSteelBlue,
            Colors.Cyan,
            Colors.DarkKhaki,
            Colors.DarkTurquoise,
            Colors.DarkGray,
            Colors.DarkOrange,
            Colors.Fuchsia,
            Colors.Navy,
            Colors.DarkOliveGreen
        };


        /// <summary>
        /// List used to store the colors that have already been used, to prevent button pairs from not having unique colors
        /// </summary>
        private readonly List<Color> sbUsedColors = new List<Color>();


        public MainPage()
        {
            this.InitializeComponent();

            //Bind methods to enigma events
            enigma.OnWheelChange = UpdateSliders;
            enigma.OnSwitchBoardLink = LinkButtons;
            enigma.OnSwitchBoardUnlink = UnlinkButtons;

            # region Insert buttons into the sbButtons dict
            //There might be a better way of doing this, but this is quick and easy for now
            
            sbBtns.Add('q', QBtn);
            sbBtns.Add('w', WBtn);
            sbBtns.Add('e', EBtn);
            sbBtns.Add('r', RBtn);
            sbBtns.Add('t', TBtn);
            sbBtns.Add('y', YBtn);
            sbBtns.Add('u', UBtn);
            sbBtns.Add('i', IBtn);
            sbBtns.Add('o', OBtn);
            sbBtns.Add('p', PBtn);
            sbBtns.Add('a', ABtn);
            sbBtns.Add('s', SBtn);
            sbBtns.Add('d', DBtn);
            sbBtns.Add('f', FBtn);
            sbBtns.Add('g', GBtn);
            sbBtns.Add('h', HBtn);
            sbBtns.Add('j', JBtn);
            sbBtns.Add('k', KBtn);
            sbBtns.Add('l', LBtn);
            sbBtns.Add('z', ZBtn);
            sbBtns.Add('x', XBtn);
            sbBtns.Add('c', CBtn);
            sbBtns.Add('v', VBtn);
            sbBtns.Add('b', BBtn);
            sbBtns.Add('n', NBtn);
            sbBtns.Add('m', MBtn);

            #endregion

            //Randomise variables
            enigma.Randomise();

            //Save our starting wheel state
            Save();
        }


        /// <summary>
        /// Update the sliders on the page with the wheel values from the enigma object
        /// </summary>
        private void UpdateSliders(int[] wheels)
        {
            //Update slider values
            W1Slider.Value = wheels[0];
            W2Slider.Value = wheels[1];
            W3Slider.Value = wheels[2];

            //Update text block values
            W1PosText.Text = wheels[0].ToString();
            W2PosText.Text = wheels[1].ToString();
            W3PosText.Text = wheels[2].ToString();
        }


        /// <summary>
        /// Assigns the same color to the switch board buttons for the given characters.
        /// </summary>
        private void LinkButtons(char a, char b) 
        {
            foreach (Color color in sbBtnColors)
            {
                if (!sbUsedColors.Contains(color))
                {
                    SolidColorBrush brush = new SolidColorBrush(color);
                    sbBtns[a].Background = brush;
                    sbBtns[b].Background = brush;
                    sbUsedColors.Add(color);
                    break;
                }
            }
        }


        /// <summary>
        /// Restores the default colors for the switch board buttons for the given characters.
        /// </summary>
        private void UnlinkButtons(char a, char b)
        {
            //Use this nasty code to get the current color and remove it from the used colors list
            Color color = ((SolidColorBrush)sbBtns[a].Background).Color;
            sbUsedColors.Remove(color);

            //Get default color from another button on the page
            sbBtns[a].Background = ProcessButton.Background;
            sbBtns[b].Background = ProcessButton.Background;
            
        }


        /// <summary>
        /// Function called upon wheel slider values changing
        /// </summary>
        private void WheelSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //Only update the value if it has actually changed
            if (e.NewValue != e.OldValue)
            {
                Slider slider = sender as Slider;
                int[] newWheels = enigma.Wheels;

                int i = (int)Char.GetNumericValue(slider.Name[1]) - 1;
                newWheels[i] = (int)e.NewValue;

                enigma.Wheels = newWheels;

                UpdateSliders(enigma.Wheels);
            }
        }


        /// <summary>
        /// Called on process button click event, processes the input from the input text box and puts the result in the output text box.
        /// </summary>
        private void ProcessInput(object sender, RoutedEventArgs e)
        {
            if (DecodeModeCheckbox.IsChecked == true)
            {
                OutputText.Text = enigma.Decode(InputText.Text);
            }
            else
            {
                OutputText.Text = enigma.Encode(InputText.Text);
            }
        }


        /// <summary>
        /// Save the current wheel and switch board states so they can be loaded later.
        /// </summary>
        private void Save()
        {
            WheelSave = enigma.Wheels;
            SwitchBoardSave = enigma.SwitchBoard;
        }


        /// <summary>
        /// Load the previously saved wheel and switch board.
        /// </summary>
        private void Load()
        {
            enigma.Wheels = WheelSave;
            enigma.SwitchBoard = SwitchBoardSave;
        }


        /// <summary>
        /// Overload of the normal SaveWheels method, which can be called from a button
        /// </summary>
        private void Save(object sender, RoutedEventArgs e)
        {
            Save();
            LoadWheelsButton.IsEnabled = true;
        }


        /// <summary>
        /// Overload of the normal LoadWheels method, which can be called from a button
        /// </summary>
        private void Load(object sender, RoutedEventArgs e)
        {
            Load();
        }


        /// <summary>
        /// Moves the contents of the output box into the input text box, and empties the output text box
        /// </summary>
        private void SwitchInput(object sender, RoutedEventArgs e)
        {
            InputText.Text = OutputText.Text;
            OutputText.Text = "";
        }


        /// <summary>
        /// Method handling switch board button linking
        /// </summary>
        private void SwitchBoardLink(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            //If switch board button is null, then this is the first press
            if (SwitchBoardButton is null)
            {
                SwitchBoardButton = button;
            }
            else
            {
                //Otherwise we must link the characters for the SwitchBoardButton and the new button
                char a = SwitchBoardButton.Content.ToString()[0];
                char b = button.Content.ToString()[0];
                enigma.LinkChars(a, b);
                SwitchBoardButton = null;
            }
        }


        /// <summary>
        /// Button action to randomise engima encoding values
        /// </summary>
        private void Randomise(object sender, RoutedEventArgs e)
        {
            enigma.Randomise();
        }
    }
}
