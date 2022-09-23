using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Concurrent;
namespace Cliente
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Thread tarea_cliente;
        static private readonly ConcurrentQueue<String> cola_tx = new ConcurrentQueue<String>();
        static private readonly ConcurrentQueue<String> cola_rx = new ConcurrentQueue<String>();
        private void Client()
        {
            IPAddress iPAddress = IPAddress.Any;
            Socket socket;
            socket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IPAddress destino = IPAddress.Parse("127.0.0.1");

                IPEndPoint endPoint = new IPEndPoint(destino, 50000);
                socket.Connect(endPoint);
                while (true)
                {
                    
                    
                    if (!cola_tx.IsEmpty)
                    {
                        
                        String dato_send;

                        cola_tx.TryDequeue(out dato_send);

                        //Enviar los datos de la variable dato
                        //byte[] bytes_send = Encoding.ASCII.GetBytes(dato_send);
                        string value = dato_send.Substring(1);
                        string command = dato_send.Substring(0, 1);
                        int valueint = int.Parse(value);

                        byte[] data = new byte[5];
                        data[0] = (byte)0x06;
                        switch(command)
                        {
                            case "F":
                                data[1] = (byte)0x20;
                                break;
                            case "A":
                                data[1] = (byte)0x30;
                                break;
                            case "W":
                                data[1] = (byte)0x60;
                                break;
                            case "P":
                                data[1] = (byte)0x40;
                                break;
                            case "IP":
                                data[1] = (byte)0x70;
                                break;
                            case "R":
                                data[1] = (byte)0x10;
                                break;
                            case "M":
                                data[1] = (byte)0x50;
                                break;
                        }
                        

                        data[2] = (byte)(valueint >> 8);
                        data[3] = (byte)(valueint >> 0);
                        data[4] = (byte)0x0a;

                        socket.Send(data);
                       
                    }
                    if (socket.Available != 0)
                    {

                        byte[] bytes_recv = new byte[64];

                        int numdata;

                        numdata = socket.Receive(bytes_recv);

                        if (numdata != 0)

                        {

                            string msg_recv;

                            msg_recv = Encoding.ASCII.GetString(bytes_recv);

                            cola_rx.Enqueue(msg_recv);

                        }

                    }
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
        }

        private void Comienzo(object sender, EventArgs e)
        {
            tarea_cliente = new Thread(new ThreadStart(Client));

            tarea_cliente.Start();
        }
        private void Insertar_cola(String dato)

        {

            cola_tx.Enqueue(dato);

        }

        private void Enviar(object sender, EventArgs e)
        {
            if(radioButton1.Checked)
            {
                cola_tx.Enqueue("F" + textBox1.Text + "\r\n");
            }
            if (radioButton2.Checked)
            {
                cola_tx.Enqueue("A" + textBox1.Text + "\r\n");
            }
            if (radioButton3.Checked)
            {
                cola_tx.Enqueue("P" + textBox1.Text + "\r\n");
            }

            textBox1.Text = "";
        }

        private void Recibir(object sender, EventArgs e)
        {
            if (!cola_rx.IsEmpty)

            {

                textBox2.Text = "";

                while (!cola_rx.IsEmpty)

                {

                    String dato;

                    cola_rx.TryDequeue(out dato);

                    textBox2.AppendText(dato + "\r\n");

                }

            }
        }
    }
}