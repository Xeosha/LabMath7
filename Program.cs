using System.Diagnostics;
using System.Text;
using LabMath7;
using Menu;

static void fun(int[,] adjacencyMatrix, VertexColor[] colors, string name)
{
    var thisPath = Directory.GetCurrentDirectory() + "\\";
    string dotFilePath = name + "graph.dot";
    string imageFilePath = name + "graph.png";

    // Создаем файл .dot для описания графа
    using (var sw = new StreamWriter(dotFilePath))
    {
        sw.WriteLine("graph G {");
        for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
        {
            sw.WriteLine($"{i} [color=\"{GetColorName(colors[i])}\"];");
            for (int j = i + 1; j < adjacencyMatrix.GetLength(1); j++)
            {
                if (adjacencyMatrix[i, j] > 0) 
                    sw.WriteLine($"{i} -- {j}");            
            }
        }
        sw.WriteLine("}");
    }

    // Запускаем Graphviz для создания изображения
    var startInfo = new ProcessStartInfo
    {
        FileName = "dot",
        Arguments = $"-Tpng {dotFilePath} -o {imageFilePath}",
        UseShellExecute = true
    };
    Process.Start(startInfo);
    System.Threading.Thread.Sleep(1000);

    // Запускаем изображение
    startInfo = new ProcessStartInfo
    {
        FileName = thisPath + imageFilePath,
        UseShellExecute = true
    };

    Process.Start(startInfo);

    Console.WriteLine($"Граф создан и сохранен как {imageFilePath}");
}

static string GetColorName(VertexColor color)
{
    switch (color)
    {
        case VertexColor.Red:
            return "red";
        case VertexColor.Blue:
            return "blue";
        case VertexColor.Green:
            return "green";
        case VertexColor.Yellow:
            return "yellow";
        case VertexColor.Orange:
            return "orange";
        case VertexColor.Purple:
            return "purple";
        case VertexColor.Gray:
            return "gray";
        default:
            return "black"; 
    }
}
static string[] GetNameTxtFiles()
{
    var folderPath = Directory.GetCurrentDirectory();

    string[] files = Directory.GetFiles(folderPath, "*.txt");

    for (int i = 0; i < files.Length; i++)
    {
        files[i] = Path.GetFileName(files[i]);
    }

    return files;
}

static int[,] ReadMatrixFromFile(string filename)
{
    string[] lines = File.ReadAllLines(filename);
    int rows = lines.Length;
    int columns = lines[0].Split(' ').Length;
    int[,] matrix = new int[rows, columns];

    for (int i = 0; i < rows; i++)
    {
        string[] values = lines[i].Split(' ');

        for (int j = 0; j < columns; j++)
        {
            matrix[i, j] = int.Parse(values[j]);
        }
    }

    return matrix;
}

static int[,]? GetMatrixFromFolder()
{
    var files = GetNameTxtFiles();
    var folderPath = Directory.GetCurrentDirectory();

    int[,]? result = null;

    var dialog = new Dialog("Выберите файлы из данной папки");
    foreach (var file in files)
    {
        var fullPath = Path.Combine(folderPath, file);
        dialog.AddOption(file, () => { result = ReadMatrixFromFile(fullPath); dialog.Close(); });
    }
    dialog.AddOption("Выход", () => dialog.Close());

    dialog.Start();

    return result;
}


static string GetString(int[,] matrix)
{
    var stringBuilder = new StringBuilder();
    int width = matrix.GetLength(1);
    int height = matrix.GetLength(0);
    stringBuilder.AppendLine("┌" + new string('─', width * 4 - 1) + "┐");
    for (int i = 0; i < height; i++)
    {
        stringBuilder.Append("│ ");
        for (int j = 0; j < width; j++)
        {
            if (matrix[j, i] == 0)
                stringBuilder.Append($"X │ ");
            else
                stringBuilder.Append($"{matrix[i, j]} │ ");
        }
        stringBuilder.AppendLine();

        if (i == height - 1)
        {
            stringBuilder.AppendLine("└" + new string('─', width * 4 - 1) + "┘");
        }
        else
        {
            stringBuilder.AppendLine("├" + new string('─', width * 4 - 1) + "┤");
        }
    }
    return stringBuilder.ToString();
}


static void PrintColorGraph(int[,]? matrix)
{
    if (matrix == null)
    {
        Console.WriteLine("Матрица смежности не была создана.");
        return;
    }

    var colors = GetColorizeGraph(matrix);

    // вывод матрицы смежности и png графа
    Console.WriteLine(GetString(matrix));
    fun(matrix, colors, "GRAPH.txt");

    Console.WriteLine("Результат раскраски дерева:");
    for (int i = 0; i < colors.Length; i++)
        Console.WriteLine($"Вершина {i}: Цвет {colors[i]}");
    
    Console.WriteLine($"Всего использовано цветов: {(int)colors.Max()}");
}

static VertexColor[] GetColorizeGraph(int[,] adjacencyMatrix)
{
    VertexColor[] colors = new VertexColor[adjacencyMatrix.GetLength(0)]; // Массив цветов для каждой вершины
    int maxColor = 0; // Максимальный использованный цвет

    for (int i = 0; i < colors.Length; i++)
    {
        // Находим минимальный неиспользованный цвет для текущей вершины
        int minAvailableColor = 1;
        while (true)
        {
            bool colorAvailable = true;
            for (int j = 0; j < adjacencyMatrix.GetLength(0); j++)
            {
                if (adjacencyMatrix[i, j] > 0 && colors[j] == (VertexColor)minAvailableColor)
                {
                    colorAvailable = false;
                    break;
                }
            }

            if (colorAvailable)
                break;

            minAvailableColor++;
        }

        colors[i] = (VertexColor)minAvailableColor;
        if (minAvailableColor > maxColor)
            maxColor = minAvailableColor;
    }

    return colors;
}

int[,]? matrix = null;

var dialog = new Dialog("Алгоритм раскраски графа");
dialog.AddOption("Создание матрицы смежности", () => matrix = GetMatrixFromFolder(), true);
dialog.AddOption("Раскраска графа", () => PrintColorGraph(matrix));

dialog.Start();