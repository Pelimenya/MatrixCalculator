using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatrixCalculator.Exceptions;
using MatrixCalculator.Models;

namespace MatrixCalculator.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _matrixAText;
    [ObservableProperty] private string _matrixBText;
    [ObservableProperty] private string _resultText;
    [ObservableProperty] private string _statusMessage;
    [ObservableProperty] private string _scalarValue = "1";
    [ObservableProperty] private string _powerValue = "2";


    private const string ParseHint =
        "Формат: строки через новую строку или ';'. Числа в строке разделяются пробелом или запятой.";

    public MainWindowViewModel()
    {
        ResultText = "";
        StatusMessage = ParseHint;
    }

    private Matrix ParseMatrix(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new InvalidMatrixFormatException("Пустой ввод матрицы.");
        var lines = text.Split(new[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Trim())
                        .Where(l => l.Length > 0)
                        .ToArray();
        if (lines.Length == 0) throw new InvalidMatrixFormatException("Не обнаружено строк матрицы.");

        double[][] rows = new double[lines.Length][];
        int cols = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            var parts = lines[i].Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (cols == -1) cols = parts.Length;
            if (parts.Length != cols)
                throw new InvalidMatrixFormatException(
                    $"Неправильная длина строки {i + 1} (ожидалось {cols}, получено {parts.Length}).");
            rows[i] = new double[cols];
            for (int j = 0; j < cols; j++)
            {
                if (!double.TryParse(parts[j], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double val))
                    throw new InvalidMatrixFormatException(
                        $"Не удалось распознать число в строке {i + 1}, колонке {j + 1}: '{parts[j]}'");
                rows[i][j] = val;
            }
        }

        return Matrix.FromArray(rows);
    }

    private void ShowResult(Matrix m)
    {
        ResultText = m.ToString();
        StatusMessage = "Готово.";
    }

    private void ShowMessage(string msg)
    {
        StatusMessage = msg;
    }

    [RelayCommand]
    private void Add()
    {
        try
        {
            var A = ParseMatrix(MatrixAText);
            var B = ParseMatrix(MatrixBText);
            ShowResult(A.Add(B));
        }
        catch (MatrixException me) { ShowMessage(me.Message); }
        catch (Exception ex) { ShowMessage(ex.Message); }
    }

    [RelayCommand]
    private void Subtract()
    {
        try
        {
            var A = ParseMatrix(MatrixAText);
            var B = ParseMatrix(MatrixBText);
            ShowResult(A.Subtract(B));
        }
        catch (MatrixException me) { ShowMessage(me.Message); }
        catch (Exception ex) { ShowMessage(ex.Message); }
    }

    [RelayCommand]
    private void Multiply()
    {
        try
        {
            var A = ParseMatrix(MatrixAText);
            var B = ParseMatrix(MatrixBText);
            ShowResult(A.Multiply(B));
        }
        catch (MatrixException me) { ShowMessage(me.Message); }
        catch (Exception ex) { ShowMessage(ex.Message); }
    }

    [RelayCommand]
    private void ScalarMultiply()
    {
        try
        {
            var A = ParseMatrix(MatrixAText);
            if (!double.TryParse(ScalarValue, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double s))
            {
                ShowMessage("Неверный скаляр.");
                return;
            }
            ShowResult(A.Multiply(s));
        }
        catch (MatrixException me) { ShowMessage(me.Message); }
        catch (Exception ex) { ShowMessage(ex.Message); }
    }

    [RelayCommand]
    private void Transpose()
    {
        try { ShowResult(ParseMatrix(MatrixAText).Transpose()); }
        catch (MatrixException me) { ShowMessage(me.Message); }
        catch (Exception ex) { ShowMessage(ex.Message); }
    }

    [RelayCommand]
    private void Determinant()
    {
        try
        {
            var det = ParseMatrix(MatrixAText).Determinant();
            ResultText = det.ToString("G8");
            StatusMessage = "Готово.";
        }
        catch (MatrixException me) { ShowMessage(me.Message); }
        catch (Exception ex) { ShowMessage(ex.Message); }
    }

    [RelayCommand]
    private void Inverse()
    {
        try { ShowResult(ParseMatrix(MatrixAText).Inverse()); }
        catch (MatrixException me) { ShowMessage(me.Message); }
        catch (Exception ex) { ShowMessage(ex.Message); }
    }

    [RelayCommand]
    private void Rank()
    {
        try
        {
            var r = ParseMatrix(MatrixAText).Rank();
            ResultText = r.ToString();
            StatusMessage = "Готово.";
        }
        catch (MatrixException me) { ShowMessage(me.Message); }
        catch (Exception ex) { ShowMessage(ex.Message); }
    }

    [RelayCommand]
    private void Power()
    {
        try
        {
            var A = ParseMatrix(MatrixAText);
            if (!int.TryParse(PowerValue, out int p))
            {
                ShowMessage("Степень должна быть целым числом.");
                return;
            }
            ShowResult(A.Power(p));
        }
        catch (MatrixException me) { ShowMessage(me.Message); }
        catch (Exception ex) { ShowMessage(ex.Message); }
    }

    [RelayCommand]
    private void ShowHelp()
    {
        ShowMessage(ParseHint);
    }
}
