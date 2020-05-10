﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace nullDCNetplayLauncher
{
    public partial class NetplayLaunchForm : Form
    {
        Launcher launcher;
        Dictionary<String, String> romDict;
        ConnectionPresetList presets;

        public NetplayLaunchForm()
        {
            launcher = new Launcher();
            romDict = ScanRoms();
            presets = ConnectionPreset.ReadPresetsFile();
            InitializeComponent();

            cboGameSelect.DataSource = new BindingSource(romDict, null);
            cboGameSelect.DisplayMember = "Key";
            cboGameSelect.ValueMember = "Value";

            if (romDict.Count == 1 && romDict.First().Key == "")
            {
                btnOffline.Enabled = false;
                btnHost.Enabled = false;
                btnJoin.Enabled = false;
                cboGameSelect.Enabled = false;
            }

            cboPresetName.DataSource = presets.ConnectionPresets;

            btnDeletePreset.Enabled = presets.ConnectionPresets.Count > 1;
        }

        private void btnOffline_Click(object sender, EventArgs e)
        {
            Launcher.UpdateCFGFile(
                netplayEnabled: false,
                frameDelay: Convert.ToInt32(numDelay.Value)
                                   .ToString());
            Launcher.LaunchNullDC(cboGameSelect.SelectedValue.ToString());
        }

        private void btnHost_Click(object sender, EventArgs e)
        {
            Launcher.UpdateCFGFile(
                netplayEnabled: true,
                isHost: true,
                hostAddress: txtIP.Text,
                hostPort: txtPort.Text,
                frameDelay: Convert.ToInt32(numDelay.Value)
                                   .ToString());
            Launcher.LaunchNullDC(cboGameSelect.SelectedValue.ToString(), isHost: true);
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            Launcher.UpdateCFGFile(
                netplayEnabled: true,
                isHost: false,
                hostAddress: txtIP.Text,
                hostPort: txtPort.Text,
                frameDelay: Convert.ToInt32(numDelay.Value)
                                   .ToString());
            Launcher.LaunchNullDC(cboGameSelect.SelectedValue.ToString());
        }

        private void LoadPreset(string presetName)
        {
            ConnectionPreset toLoad = presets.ConnectionPresets.FirstOrDefault(p => p.Name == presetName);
            if (toLoad != null)
            {
                txtIP.Text = toLoad.IP;
                txtPort.Text = toLoad.Port;
                numDelay.Value = toLoad.Delay;
            }
        }

        public void SavePreset(string presetName)
        {
            var toEdit = presets.ConnectionPresets.FirstOrDefault(p => p.Name == presetName);
            if (toEdit != null)
            {
                toEdit.IP = txtIP.Text;
                toEdit.Port = txtPort.Text;
                toEdit.Delay = numDelay.Value;
            }
            else
            {
                var toAdd = new ConnectionPreset();
                toAdd.Name = cboPresetName.Text;
                toAdd.IP = txtIP.Text;
                toAdd.Port = txtPort.Text;
                toAdd.Delay = numDelay.Value;
                presets.ConnectionPresets.Add(toAdd);
            }

            var path = Launcher.GetApplicationConfigurationDirectoryName() + "//ConnectionPresetList.xml";
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(ConnectionPresetList));
            StreamWriter writer = new StreamWriter(path);
            serializer.Serialize(writer.BaseStream, presets);
            writer.Close();
            presets = ConnectionPreset.ReadPresetsFile();
            cboPresetName.DataSource = presets.ConnectionPresets;
            cboPresetName.SelectedIndex = cboPresetName.FindStringExact(presetName);
            if (presets.ConnectionPresets.Count > 1)
            {
                btnDeletePreset.Enabled = true;
            }
        }

        public void DeletePreset(string presetName)
        {
            if (presets.ConnectionPresets.Count > 1)
            {
                var toDelete = presets.ConnectionPresets.FirstOrDefault(p => p.Name == presetName);
                presets.ConnectionPresets.Remove(toDelete);

                var path = Launcher.GetApplicationConfigurationDirectoryName() + "//ConnectionPresetList.xml";
                System.Xml.Serialization.XmlSerializer serializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(ConnectionPresetList));
                StreamWriter writer = new StreamWriter(path);
                serializer.Serialize(writer.BaseStream, presets);
                writer.Close();
                presets = ConnectionPreset.ReadPresetsFile();
                cboPresetName.DataSource = presets.ConnectionPresets;
                cboPresetName.SelectedIndex = 0;
                if (presets.ConnectionPresets.Count == 1)
                {
                    btnDeletePreset.Enabled = false;
                }
            }
        }

        private Dictionary<string, string> ScanRoms()
        {
            string RomDir = Launcher.rootDir + "nulldc-1-0-4-en-win\\roms\\";
            Dictionary<string, string> romDict;
            try
            {
                List<string> romPaths = Directory.GetFiles(RomDir, "*?.lst", SearchOption.AllDirectories).Where(item => item.EndsWith(".lst")).ToList();
                romDict = romPaths
                    .ToDictionary(x => Launcher.ExtractRomNameFromPath(x), x => Launcher.ExtractRelativeRomPath(x));
            }
            catch (Exception)
            {
                List<string> romPaths = new List<string>();
                romPaths.Add("");
                romDict = romPaths.ToDictionary(x => x, x => x);
            }
            return romDict;
        }

        private void btnSavePreset_Click(object sender, EventArgs e)
        {
            SavePreset(cboPresetName.Text);
        }

        private void btnDeletePreset_Click(object sender, EventArgs e)
        {
            DeletePreset(cboPresetName.Text);
        }

        private void cboPresetName_TextChanged(object sender, EventArgs e)
        {
            LoadPreset(cboPresetName.Text);
        }

    }
}