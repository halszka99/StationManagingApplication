using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Projekt2.Models;

namespace Projekt2
{
    public partial class Form1 : Form
    {
        Station station;
        public Form1()
        {
            InitializeComponent();
            station = new Station(this);
            buttonStop.Enabled = false;
        }
        public List<TextBox> JunctionTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>();
            textBoxes.Add(textBox1);
            textBoxes.Add(textBox2);
            return textBoxes; 
        }
        public List<TextBox> LeftEntryTracksTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>();
            textBoxes.Add(textBox3);
            textBoxes.Add(textBox4);
            textBoxes.Add(textBox5);
            textBoxes.Add(textBox6);
            return textBoxes;
        }        
        public List<TextBox> RightEntryTracksTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>();
            textBoxes.Add(textBox11);
            textBoxes.Add(textBox12);
            textBoxes.Add(textBox13);
            textBoxes.Add(textBox14);
            return textBoxes;
        }
        public List<TextBox> PlatformTracksTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>();
            textBoxes.Add(textBox7);
            textBoxes.Add(textBox8);
            textBoxes.Add(textBox9);
            textBoxes.Add(textBox10);
            return textBoxes;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            station.StartSimulation();
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            station.EndSimulation();
            buttonStop.Enabled = false;
        }
        public void Ready()
        {

            buttonStart.Enabled = true;
            textBox1.Text = "Free"; 
            textBox2.Text = "Free"; 
            textBox3.Text = "Free"; 
            textBox4.Text = "Free"; 
            textBox5.Text = "Free"; 
            textBox6.Text = "Free"; 
            textBox7.Text = "Free"; 
            textBox8.Text = "Free"; 
            textBox9.Text = "Free"; 
            textBox10.Text = "Free"; 
            textBox11.Text = "Free"; 
            textBox12.Text = "Free"; 
            textBox13.Text = "Free"; 
            textBox14.Text = "Free"; 
        }
    }
}
