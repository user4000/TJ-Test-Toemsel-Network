﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bogus;
using Telerik.WinControls;
using Telerik.WinControls.UI;
using Telerik.WinControls.UI.Docking;
using TJFramework;
using TJSettings;
using TJStandard;
using TJStandard.Tools;
using static TestSettingsConsumer.Program;
using static TJFramework.TJFrameworkManager;

namespace TestSettingsConsumer
{
  public partial class FormTest2 : RadForm, IEventStartWork
  {

    private List<Folder> ListFolder = new List<Folder>();

    private List<Setting> ListSetting = new List<Setting>();

    public FormTest2()
    {
      InitializeComponent();
    }

    public void EventStartWork()
    {
      Trace.WriteLine("--> #FormTest2# [EventStartWork]");
      SetEvents();
      Trace.WriteLine("<-- #FormTest2# [EventStartWork]");
    }

    private void SetEvents()
    {
      BxListAllFolders.Click += EventGetListOfFolders;
      BxGetChildrenOfOneFolder.Click += EventGetChildrenOfOneFolder;
      BxGetIdFolder.Click += EventGetIdFolder;
      BxGetListOfSettings.Click += EventGetListOfSettings;
      BxDeleteSettings.Click += EventDeleteSettings;
      BxFolderForceDelete.Click += EventFolderForceDelete;
      BxForceDeleteFolderUsingTreeview.Click += EventFolderForceDelete;
    }

    private void PrintInner(string message)
    {
      TxMessage.AppendText(message + Environment.NewLine);
    }

    public void Print(string message)
    {
      if (TxMessage.InvokeRequired)
        TxMessage.Invoke((MethodInvoker)delegate { PrintInner(message); });
      else
        PrintInner(message);
    }

    private void EventGetListOfFolders(object sender, EventArgs e)
    {
      TxMessage.Clear();
      string names = string.Empty;
      List<Folder> list = DbSettings.GetListOfFolders();
      foreach (var item in list) names += item.IdFolder + " = " + item.FullPath + Environment.NewLine;
      TxMessage.Text = names;
    }

    private void EventGetChildrenOfOneFolder(object sender, EventArgs e)
    {
      string ParentFolderFullPath = TxOne.Text;
      List<string> list = DbSettings.GetListOfFolders(ParentFolderFullPath);
      string names = string.Empty;
      foreach (var item in list) names += item + Environment.NewLine;
      Print($"-------------------- {ParentFolderFullPath}");
      Print(names);
      TxTwo.Text = ParentFolderFullPath;
      TxOne.Clear();
    }

    private void EventGetIdFolder(object sender, EventArgs e)
    {
      string FolderFullPath = TxOne.Text;
      TxTwo.Text = FolderFullPath;
      int IdFolder = DbSettings.GetIdFolder(FolderFullPath);
      TxOne.Text = IdFolder.ToString();
    }

    private void EventGetListOfSettings(object sender, EventArgs e)
    {
      string FolderFullPath = TxOne.Text;
      TxTwo.Text = FolderFullPath;
      TxOne.Clear();
      List<string> list = DbSettings.GetSettings(FolderFullPath, TypeSetting.Text);
      string names = string.Empty;
      foreach (var item in list) names += item + Environment.NewLine;
      Print($"SETTINGS OF A FOLDER = {FolderFullPath}");
      Print(names);
    }

    private void EventDeleteSettings(object sender, EventArgs e)
    {
      string FolderFullPath = TxOne.Text;
      TxTwo.Text = FolderFullPath;
      TxOne.Clear();
      ReturnCode code = DbSettings.DeleteAllSettingsOfOneFolder(FolderFullPath);
      Print(ReturnCodeFormatter.ToString(code));
    }

    private async void EventFolderForceDelete(object sender, EventArgs e)
    {
      string FolderFullPath = TxOne.Text;
      TxTwo.Text = FolderFullPath;
      TxOne.Clear();

      if (FolderFullPath.Trim().Length < 1)
      {
        Print("ERROR ! Folder name is empty");
        return;
      }

      Print("");
      Print("---------------------------------------------------------------------------------------------");
      BxFolderForceDelete.Enabled = false;
      BxForceDeleteFolderUsingTreeview.Enabled = false;

      Application.DoEvents();

      Stopwatch sw;

      await Task.Delay(500);

      Application.DoEvents();

      ReturnCode code;
      if ((sender as RadButton).Name == BxFolderForceDelete.Name)
      {
        Print(" * * *  FolderForceDelete");
        sw = Stopwatch.StartNew();
        code = DbSettings.FolderForceDelete(FolderFullPath);
      }
      else
      {
        Print(" ^ ^ ^  FolderForceDeleteUsingTreeview");
        sw = Stopwatch.StartNew();
        code = DbSettings.FolderForceDeleteUsingTreeview(FolderFullPath);
      }
      sw.Stop();

      BxFolderForceDelete.Enabled = true;
      BxForceDeleteFolderUsingTreeview.Enabled = true;

      Application.DoEvents();

      Print(ReturnCodeFormatter.ToString(code));
      Print($"Time = {sw.ElapsedMilliseconds} ms");

      await Task.Delay(500);

      code = DbSettings.FolderDelete(FolderFullPath, FolderFullPath);

      Print($"CHECK is folder deleted: {ReturnCodeFormatter.ToString(code)}");

      CxProcess.Execute(@"e:\restore_test_db.bat", "");
    }
  }
}
