using System.Text.RegularExpressions;
using Avalonia.Controls;

namespace BililiveRecorder.Avalonia.Pages;

public partial class AboutPage : UserControl
{
    public AboutPage()
    {
        InitializeComponent();
        if (!string.IsNullOrEmpty(GitVersionInformation.CommitDate))
        {
            var match = Regex.Match(GitVersionInformation.CommitDate,
                @"^(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})$");
            if (match.Success)
            {
                CopyrightTextBlock.Text = $" © {match.Groups["year"].Value} Genteure";
            }
        }
    }
}
