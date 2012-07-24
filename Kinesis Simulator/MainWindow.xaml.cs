using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ServiceProcess;
using Newtonsoft.Json;

using System.Security.Principal;
using System.Globalization;
using System.Collections;

using AppLimit.NetSparkle;

namespace Kinesis_Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Sparkle _sparkle;

        public static Server server;
        public MainWindow()
        {
            InitializeComponent();
            //_sparkle = new Sparkle("http://download.kinesis.io/win_simulator/appcast.xml");
            //_sparkle.StartLoop(true);
            //server = new Server();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Start_Server.IsEnabled = true;
            Stop_Server.IsEnabled = false;

            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) ? true : false;
            if (isAdmin)
            {
            }
            else
            {
                MessageBox.Show("Please run the Kinesis Simulator as an Administrator.");
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (server != null)
            {
                server.Stop();
            }
        }

        public static bool IsServiceInstalled(string serviceName)
        {
            // get list of Windows services

            ServiceController[] services = ServiceController.GetServices();

            // try to find service name

            foreach (ServiceController service in services)
            {
                if (service.ServiceName == serviceName)
                    return true;
            }
            return false;
        }

        public static bool StartService(string serviceName, int timeoutMilliseconds)
        {
            if (!IsServiceInstalled(serviceName))
                return true;

            ServiceController service = new ServiceController(serviceName);
            if (service == null)
                return true;

            if (service.Status == ServiceControllerStatus.Running)
                return true;

            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show(e.ToString());
                MessageBox.Show("Error: Please run the simulator as an Administrator");
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
            return true;
        }

        public static bool StopService(string serviceName, int timeoutMilliseconds)
        {
            if (!IsServiceInstalled(serviceName))
                return true;

            ServiceController service = new ServiceController(serviceName);

            if (service == null)
                return true;
            
            
            if (service.Status == ServiceControllerStatus.Stopped)
                return true;

            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show(e.ToString());
                MessageBox.Show("Error: Please run the simulator as an Administrator");
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
            return true;
        }

        private void Start_Server_Click(object sender, RoutedEventArgs e)
        {
            if (StopService("Kinesis.io_Service", 5000))
            {
                server.Start();
                Start_Server.IsEnabled = false;
                Stop_Server.IsEnabled = true;
            }
        }

        public void Stop_Event()
        {
            if (StartService("Kinesis.io_Service", 5000))
            {
                if (server != null)
                {
                    server.Stop();
                    Start_Server.IsEnabled = true;
                    Stop_Server.IsEnabled = false;
                }
            }
        }

        private void Stop_Server_Click(object sender, RoutedEventArgs e)
        {
             this.Stop_Event();     
        }

        private void Speech_Send_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll(String.Format("{{\"gestures\":[{{\"type\":4,\"command\":\"{0}\"}}],\"cursor\":{{\"x\":50,\"y\":50,\"z\":50}}}}", textBox1.Text));
        }


        private void Left_SwipeLeft_Click(object sender, RoutedEventArgs e)
        {
            sendGesture(0, 8, 0);
        }

        private void Left_SwipeRight_Click(object sender, RoutedEventArgs e)
        {
            sendGesture(0, 8, 1);
        }

        private void Left_SwipeUp_Click(object sender, RoutedEventArgs e)
        {
            sendGesture(0, 8, 2);
        }

        private void Left_SwipeDown_Click(object sender, RoutedEventArgs e)
        {
            sendGesture(0, 8, 3);
        }

        private void Right_SwipeLeft_Click(object sender, RoutedEventArgs e)
        {
            sendGesture(0, 0, 0);
        }

        private void Right_SwipeRight_Click(object sender, RoutedEventArgs e)
        {
            sendGesture(0, 0, 1);
        }

        private void Right_SwipeUp_Click(object sender, RoutedEventArgs e)
        {
            sendGesture(0, 0, 2);
        }

        private void Right_SwipeDown_Click(object sender, RoutedEventArgs e)
        {
            sendGesture(0, 0, 3);
        }

        private void ellipse1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = Mouse.GetPosition(canvas1);
            Trace.WriteLine(mousePos.ToString());
            Canvas.SetLeft(ellipse1, mousePos.X);
            Canvas.SetTop(ellipse1, mousePos.Y);
        }

        bool mouseClicked = false;

        private void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseClicked == false)
                return;

            Point mousePos = Mouse.GetPosition(canvas1);
            if (mousePos.X > 200 || mousePos.Y > 200 || mousePos.X < 0 || mousePos.Y < 0)
            {
                mouseClicked = false;
                return;
            }      
            Canvas.SetLeft(ellipse1, mousePos.X-10);
            Canvas.SetTop(ellipse1, mousePos.Y-10);

            Hashtable position = new Hashtable();
            position.Add("x", mousePos.X / 2);
            position.Add("y", mousePos.Y / 2);
            position.Add("z", depthValue.Content);

            Hashtable message = new Hashtable();
            message.Add("cursor", position);
            server.SendToAll(JsonConvert.SerializeObject(message));
        }

        private void canvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseClicked = true;
        }

        private void canvas1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseClicked = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop_Event();
        }

        private void Left_holdLeft_Click(object sender, RoutedEventArgs e)
        {
            this.sendGesture(2, 8, 0);
        }

        private void sendGesture(int type, int joint, int direction)
        {
            server.SendToAll(String.Format("{{\"gestures\":[{{\"type\":{0},\"joints\":[{1}],\"direction\":{2},\"origin\":{{\"x\":{3},\"y\":{4},\"z\":{5}}}}}],\"cursor\":{{\"x\":50,\"y\":50,\"z\":50}}}}", type, joint, direction, originalX.Text, originalY.Text, originalZ.Text));
        }

        private void Left_holdUp_Click(object sender, RoutedEventArgs e)
        {
            sendGesture(2, 8, 2);
        }

        private void Left_holdFront_Click(object sender, RoutedEventArgs e)
        {
         
            sendGesture(2, 8, 4);
        }

        private void Right_holdRight_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"gestures\":[{\"type\":2,\"joints\":[0],\"direction\":1}],\"cursor\":{\"x\":50,\"y\":50,\"z\":50}}");
        }

        private void Right_holdUp_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"gestures\":[{\"type\":2,\"joints\":[0],\"direction\":2}],\"cursor\":{\"x\":50,\"y\":50,\"z\":50}}");
        }

        private void Right_holdFront_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"gestures\":[{\"type\":2,\"joints\":[0],\"direction\":4}],\"cursor\":{\"x\":50,\"y\":50,\"z\":50}}");
        }

        private void leanLeft_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"gestures\":[{\"type\":1,\"subtype\":0,\"direction\":0}],\"cursor\":{\"x\":50,\"y\":50,\"z\":50}}");
        }

        private void leanRight_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"gestures\":[{\"type\":1,\"subtype\":0,\"direction\":1}],\"cursor\":{\"x\":50,\"y\":50,\"z\":50}}");
        }

        private void leanFront_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"gestures\":[{\"type\":1,\"subtype\":0,\"direction\":4}],\"cursor\":{\"x\":50,\"y\":50,\"z\":50}}");
        }

        private void Jump_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"gestures\":[{\"type\":1,\"subtype\":0,\"direction\":2}],\"cursor\":{\"x\":50,\"y\":50,\"z\":50}}");
        }

        private void Crouch_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"gestures\":[{\"type\":1,\"subtype\":0,\"direction\":3}],\"cursor\":{\"x\":50,\"y\":50,\"z\":50}}");
        }

        private void turnLeft_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"gestures\":[{\"type\":1,\"subtype\":1,\"direction\":0}],\"cursor\":{\"x\":50,\"y\":50,\"z\":50}}");
        }

        private void turnRight_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"gestures\":[{\"type\":1,\"subtype\":1,\"direction\":1}],\"cursor\":{\"x\":50,\"y\":50,\"z\":50}}");
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            depthValue.Content = slider1.Value.ToString();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"Kinect\":\"Connected\"}");
            button2.IsEnabled = true;
            button1.IsEnabled = false;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            server.SendToAll("{\"Kinect\":\"Disconnected\"}");
            button2.IsEnabled = false;
            button1.IsEnabled = true;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://kinesis.io/download/");
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Kinesis-io/examples");
        }
    }
}
