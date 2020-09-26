using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Enigma enigma;
        private int[] WheelStartState;

        public MainPage()
        {
            this.InitializeComponent();
            enigma = new Enigma();

            //Bind methods to enigma events
            enigma.OnWheelAdvance = UpdateSliders;

            //Set the starting slider positions
            UpdateSliders();

            //Save our starting wheel state
            SaveWheels();
        }

        /// <summary>
        /// Update the sliders on the page with the wheel values from the enigma object
        /// </summary>
        private void UpdateSliders()
        {
            //Update slider values
            W1Slider.Value = enigma.Wheels[0];
            W2Slider.Value = enigma.Wheels[1];
            W3Slider.Value = enigma.Wheels[2];

            //Update text block values
            W1PosText.Text = enigma.Wheels[0].ToString();
            W2PosText.Text = enigma.Wheels[1].ToString();
            W3PosText.Text = enigma.Wheels[2].ToString();
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

                UpdateSliders();
            }
        }


        /// <summary>
        /// Called on process button click event, processes the input from the input text box and puts the result in the output text box.
        /// </summary>
        private void ProcessInput(object sender, RoutedEventArgs e)
        {
            if (DecodeModeCheckbox.IsChecked == true)
            {
                OutputText.Text = enigma.Encode(InputText.Text);
            } 
            else
            {
                OutputText.Text = enigma.Decode(InputText.Text);
            }
        }


        /// <summary>
        /// Save the current wheel states to be loaded later.
        /// </summary>
        private void SaveWheels()
        {
            WheelStartState = enigma.Wheels;
        }


        /// <summary>
        /// Load the previously saved wheel states.
        /// </summary>
        private void LoadWheels()
        {
            enigma.Wheels = WheelStartState;
            UpdateSliders();
        }


        /// <summary>
        /// Overload of the normal SaveWheels method, which can be called from a button
        /// </summary>
        private void SaveWheels(object sender, RoutedEventArgs e)
        {
            SaveWheels();
            LoadWheelsButton.IsEnabled = true;
        }


        /// <summary>
        /// Overload of the normal LoadWheels method, which can be called from a button
        /// </summary>
        private void LoadWheels(object sender, RoutedEventArgs e)
        {
            LoadWheels();
        }


        /// <summary>
        /// Moves the contents of the output box into the input text box, and empties the output text box
        /// </summary>
        private void SwitchInput(object sender, RoutedEventArgs e)
        {
            InputText.Text = OutputText.Text;
            OutputText.Text = "";
        }
    }
}
