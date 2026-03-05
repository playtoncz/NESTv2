using Avalonia.Controls;
using NestCore.Model;

namespace NestStudio;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainContent.Content = new Views.WelcomeView(this);
    }

    public void ShowConsultation(KnowledgeBase kb, string? basePath, ConsultationRunConfig? runConfig = null)
    {
        MainContent.Content = new Views.ConsultationView(kb, basePath, this, runConfig);
    }

    public void ShowKbEditor(KnowledgeBase kb, string? basePath)
    {
        MainContent.Content = new Views.KbEditorView(kb, basePath, this);
    }

    public void ShowWelcome()
    {
        MainContent.Content = new Views.WelcomeView(this);
    }
}
