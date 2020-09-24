using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public MainPage()
        {
            this.InitializeComponent();
            enigma = new Enigma();

            int[] wheels = enigma.Wheels;
            String encoded = enigma.Encode("enigma");
            enigma.Wheels = wheels;
            String decoded = enigma.Decode(encoded);

            Debug.WriteLine(String.Format("Wheels: {0}, {1}, {2}", wheels[0], wheels[1], wheels[2]));
            Debug.WriteLine("Encoded: " + encoded);
            Debug.WriteLine("Decoded: " + decoded);
        }
    }
}
