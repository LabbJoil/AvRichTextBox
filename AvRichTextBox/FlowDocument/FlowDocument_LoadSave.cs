using DocumentFormat.OpenXml.Packaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using static AvRichTextBox.WordConversions;
using static AvRichTextBox.RtfConversions;
using static AvRichTextBox.XamlConversions;
using RtfDomParserAv;
using System.Text;
using HtmlAgilityPack;
using System.Threading.Tasks;

namespace AvRichTextBox;

public partial class FlowDocument
{
	internal async Task LoadRtf(string rtfContent)
	{
		RTFDomDocument rtfdom = new();

		// Do this to fix malformed `\o "` and orphaned quotes
		if (rtfContent.Contains("\\o "))
		{
			rtfContent = rtfContent.Replace("\\o \"}", "\\o \"\"}").Replace(" \"}", " }");
			rtfContent = Regex.Replace(rtfContent, "\\\\o \".*?\"", "\\o\"\"");
		}
		using MemoryStream rtfStream = new(Encoding.UTF8.GetBytes(rtfContent));
		using StreamReader streamReader = new(rtfStream);

		rtfdom.Load(streamReader.BaseStream);

		try
		{
         if (rtfdom.Elements.Count > 0)
         {
            ClearDocument();
            GetFlowDocumentFromRtf(rtfdom!, this);
            await InitializeDocument();
         }
      }
		catch (Exception ex2)
		{ 
			Debug.WriteLine($"error getting flow doc:\n{ex2.Message}");
         await NewDocument();
      }
   }
	
	internal async Task LoadRtfFromFile(string fileName)
	{
		try
		{
			string rtfContent = File.ReadAllText(fileName);
			await LoadRtf(rtfContent);
		}
		catch (Exception ex3)
		{
			if (ex3.HResult == -2147024864)
				throw new IOException($"The file:\n{fileName}\ncannot be opened because it is currently in use by another application.", ex3);
			else
				Debug.WriteLine($"Error trying to open file: {ex3.Message}");
		}
	}

	internal void SaveRtfToFile(string fileName)
	{
		try
		{
			string rtfText = SaveRtf();
			File.WriteAllText(fileName, rtfText, Encoding.Default);
			//Debug.WriteLine(rtfText);
		}
		catch (Exception ex2) { Debug.WriteLine($"error getting flow doc:\n{ex2.Message}"); }
	}
	
	internal string SaveRtf()
	{
		return GetRtfFromFlowDocument(this);
	}


	internal void SaveXamlToFile(string fileName)
	{
		File.WriteAllText(fileName, SaveXaml());
	}

	internal async Task LoadXamlFromFile(string fileName)
	{
		string xamlDocString = File.ReadAllText(fileName);
		await LoadXaml(xamlDocString);
	}

	internal string SaveXaml()
	{
		return GetDocXaml(false, this);
	}

	internal async Task LoadXaml(string xamlContent)
	{
		ProcessXamlString(xamlContent, this);
		await InitializeDocument();
	}

	internal void SaveHtmlDocToFile(string fileName)
	{
		HtmlDocument hdoc = HtmlConversions.GetHtmlFromFlowDocument(this);
		hdoc.Save(fileName);
	}
	
	internal string SaveHtml()
	{
		HtmlDocument hdoc = HtmlConversions.GetHtmlFromFlowDocument(this);
		return hdoc.DocumentNode.OuterHtml;
	}

	internal async Task LoadHtmlDocFromFile(string fileName)
	{
		try
		{
			await LoadHtml(File.ReadAllText(fileName));
		}
		catch (Exception ex3)
		{
			if (ex3.HResult == -2147024864)
				throw new IOException($"The file:\n{fileName}\ncannot be opened because it is currently in use by another application.\n{ex3.Message}");
			else
				Debug.WriteLine($"Error trying to open file: {ex3.Message}");
		}

	}
	
	internal async Task LoadHtml(string htmlContent)
	{
		try
		{
			ClearDocument();
			HtmlDocument hdoc = new();
			hdoc.LoadHtml(htmlContent);
			HtmlConversions.GetFlowDocumentFromHtml(hdoc, this);
			await InitializeDocument();
		}
		catch (Exception ex2) { Debug.WriteLine("error getting flow doc:\n" + ex2.Message); }
	}


	internal void SaveWordDocToFile(string fileName)
	{
		WordConversions.SaveWordDoc(fileName, this);
	}

	internal async Task LoadWordDocFromFile(string fileName)
	{
		try
		{
			using WordprocessingDocument WordDoc = WordprocessingDocument.Open(fileName, false);
			try
			{
				ClearDocument();
				GetFlowDocument(WordDoc.MainDocumentPart!, this);
				await InitializeDocument();
			}
			catch (Exception ex2) { Debug.WriteLine("error getting flow doc:\n" + ex2.Message); }
		}
		catch (Exception ex3)
		{
			if (ex3.HResult == -2147024864)
				throw new IOException($"The file:\n{fileName}\ncannot be opened because it is currently in use by another application.", ex3);
			else
				Debug.WriteLine($"Error trying to open file: {ex3.Message}");
		}

	}

	internal async Task LoadXamlPackage(string fileName)
	{

		XamlConversions.LoadXamlPackage(fileName, this);

		await InitializeDocument();

	}


	internal void SaveXamlPackage(string fileName)
	{
		XamlConversions.SaveXamlPackage(fileName, this);
	}

}


