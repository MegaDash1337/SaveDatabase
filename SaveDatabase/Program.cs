// Папка с базами данных. Программа автоматически выберает все файлы соответствующего расширения из этой папки
const string DIR = "D:/";

// Расширение файлов для копирования
const string EXTENSION = ".db";

// Папка с сохранениями. В ней создаются подпапки (название - дата создания), в которые копируются сохранения
const string SAVE_DIR = "D:/Saves/";

// Названия папок и файлов не могут содержать ':', которое встречается во времени, поэтому оно заменяется на этот символ
const string TIME_SEPARATOR = "%";

// Период сохранения (в миллисекундах)
const int SAVE_EVERY_MS = 20_000;


// Период удаления самой ранней папки, синтаксис: (дни, часы, минуты, секунды)
var WaitBeforeDelete = new TimeSpan(0, 0, 2, 0);

if (!Directory.Exists(SAVE_DIR))
{
    Directory.CreateDirectory(SAVE_DIR);
}

while (true)
{
    var files = Directory.GetFiles(DIR).Where(file => new FileInfo(file).Extension == EXTENSION);
    var saves = Directory.GetDirectories(SAVE_DIR).ToList();

    if (saves.Any())
    {
        var lastSave = saves.OrderBy(save => DateTime.Parse(new DirectoryInfo(save).Name.Replace(TIME_SEPARATOR, ":"))).First();

        if (DateTime.Now - DateTime.Parse(new DirectoryInfo(lastSave).Name.Replace(TIME_SEPARATOR, ":")) >= WaitBeforeDelete)
        {
            try
            {
                Log($"Удаление папки: {lastSave}");
                 
                foreach (var file in Directory.GetFiles(lastSave))
                {
                    Log($"Удаление файла в папке {lastSave}: {file}...", ConsoleColor.Yellow);
                    File.Delete(file);
                    AddToLog("успешно");
                }

                Directory.Delete(lastSave, false);
                saves.Remove(lastSave);
                Log($"Папка {lastSave} удалена (успешно)");
            } 
            catch (Exception ex)
            {
                Error(ex);
            }
        }
    }

    var dirName = SAVE_DIR + DateTime.Now.ToString().Replace(":", TIME_SEPARATOR);

    Log($"Создание директории {dirName}");

    Directory.CreateDirectory(dirName);

    Log($"Директория {dirName} создана успешно");

    foreach (var file in files)
    {
        Log($"Копирование файла {file} в {dirName}...");
        try
        {
            File.Copy(file, dirName + $"/{new FileInfo(file).Name}");
            AddToLog("успешно");
        } 
        catch (Exception ex)
        {
            Error(ex);
        }
    }

    LogWithoutDate($"\nСледующее сохранение: {DateTime.Now.AddMilliseconds(SAVE_EVERY_MS)}");

    Thread.Sleep(SAVE_EVERY_MS);
}

void Log(string? message, ConsoleColor color = ConsoleColor.Green)
{
    if (message is null)
    {
        Console.WriteLine();
    }

    Console.WriteLine();
    Console.ForegroundColor = color;
    Console.Write($"[{DateTime.Now}] {message}");
}

void AddToLog(string? message)
{
    Console.Write(message);
}

void LogWithoutDate(string? message, ConsoleColor color = ConsoleColor.Green)
{
    Console.ForegroundColor = color;
    Console.Write(message);
}
 
void Error(Exception? ex)
{
    if (ex is not null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{DateTime.Now}] Ошибка: {ex.Message}, типа {ex.GetType().Name}, библиотека: {ex.GetType().Assembly}, трассировка стека: {ex.StackTrace}");
    }
}