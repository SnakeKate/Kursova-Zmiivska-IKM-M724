using Microsoft.VisualBasic.Devices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Collections.Generic;



namespace SCADA
    {
        // Датчик для моніторингу параметрів
        public class Sensor
        {
            // Тип датчика (наприклад, температура, вологість, тиск)
            public string Type { get; private set; }

            // Поточне значення, яке вимірює датчик
            public double CurrentValue { get; private set; }

            // Допустимі межі значень
            public double MinValue { get; private set; }
            public double MaxValue { get; private set; }

            // Статус датчика (чи працює нормально)
            public bool IsOperational { get; private set; }

            // Конструктор для ініціалізації датчика
            public Sensor(string type, double minValue, double maxValue)
            {
                Type = type;
                MinValue = minValue;
                MaxValue = maxValue;
                IsOperational = true; // За замовчуванням датчик працює
            }

            // Зчитування значення
            public double ReadValue()
            {
                // Для імітації роботи датчика генеруємо випадкове значення в межах
                Random random = new Random();
                CurrentValue = random.NextDouble() * (MaxValue - MinValue) + MinValue;
                return CurrentValue;
            }

            // Перевірка на вихід за межі
            public bool IsOutOfRange()
            {
                return CurrentValue < MinValue || CurrentValue > MaxValue;
            }

            // Калібрування датчика
            public void Calibrate(double newMin, double newMax)
            {
                MinValue = newMin;
                MaxValue = newMax;
                Console.WriteLine($"Sensor '{Type}' calibrated: Min={MinValue}, Max={MaxValue}");
            }
        }
    }

    namespace SCADA
    {
        // Виконавчий механізм, який змінює параметри
        public class Actuator
        {
            // Тип виконавчого механізму (наприклад, нагрівач, кулер)
            public string Type { get; private set; }

            // Поточний рівень впливу (від 0 до 100%)
            public double CurrentLevel { get; private set; }

            // Статус роботи (увімкнено/вимкнено)
            public bool IsActive { get; private set; }

            // Конструктор для створення виконавчого механізму
            public Actuator(string type)
            {
                Type = type;
                CurrentLevel = 0;
                IsActive = false;
            }

            // Увімкнення механізму
            public void Start()
            {
                IsActive = true;
                Console.WriteLine($"Actuator '{Type}' started.");
            }

            // Вимкнення механізму
            public void Stop()
            {
                IsActive = false;
                CurrentLevel = 0;
                Console.WriteLine($"Actuator '{Type}' stopped.");
            }

            // Регулювання потужності
            public void AdjustPower(double level)
            {
                if (IsActive)
                {
                    CurrentLevel = Math.Clamp(level, 0, 100); // Обмеження рівня 0-100%
                    Console.WriteLine($"Actuator '{Type}' power adjusted to {CurrentLevel}%.");
                }
                else
                {
                    Console.WriteLine($"Actuator '{Type}' is not active. Cannot adjust power.");
                }
            }
        }
    }

    namespace SCADA
    {
        // Контролер для управління виконавчими механізмами на основі даних від датчиків
        public class Controller
        {
            // Метод розрахунку дій на основі поточних і заданих параметрів
            public double CalculateAction(double currentValue, double targetValue, double kP, double kI)
            {
                // Простий PI-регулятор
                double error = targetValue - currentValue;
                double action = kP * error + kI * (error / 2); // Інтегральний компонент для спрощення
                return Math.Clamp(action, 0, 100); // Дія в межах 0-100%
            }

            // Перевірка помилок у системі
            public bool CheckForErrors(Sensor sensor)
            {
                if (sensor.IsOutOfRange())
                {
                    Console.WriteLine($"Error: Sensor '{sensor.Type}' out of range! Value={sensor.CurrentValue}");
                    return true;
                }
                return false;
            }
        }
    }

namespace SCADA
    {
        // Основна система вентиляції
        public class VentilationSystem
        {
            private List<Sensor> sensors = new List<Sensor>();
            private List<Actuator> actuators = new List<Actuator>();
            private Controller controller = new Controller();

            // Додавання датчика
            public void AddSensor(Sensor sensor)
            {
                sensors.Add(sensor);
            }

            // Додавання виконавчого механізму
            public void AddActuator(Actuator actuator)
            {
                actuators.Add(actuator);
            }

            // Запуск системи
            public void StartSystem()
            {
                Console.WriteLine("Starting ventilation system...");
                foreach (var actuator in actuators)
                {
                    actuator.Start();
                }
            }

            // Зупинка системи
            public void StopSystem()
            {
                Console.WriteLine("Stopping ventilation system...");
                foreach (var actuator in actuators)
                {
                    actuator.Stop();
                }
            }

            // Моніторинг системи
            public void MonitorSystem(double targetTemperature, double targetHumidity)
            {
                foreach (var sensor in sensors)
                {
                    sensor.ReadValue();
                    Console.WriteLine($"Sensor '{sensor.Type}' value: {sensor.CurrentValue}");
                    controller.CheckForErrors(sensor);
                }

                // Приклад використання: управління температурою
                var tempSensor = sensors.Find(s => s.Type == "Temperature");
                var heater = actuators.Find(a => a.Type == "Heater");
                if (tempSensor != null && heater != null)
                {
                    double action = controller.CalculateAction(tempSensor.CurrentValue, targetTemperature, 1.0, 0.5);
                    heater.AdjustPower(action);
                }
            }
        }
    }
namespace SCADA
    {
        // Інтерфейс для взаємодії з користувачем
        public class SCADAInterface
        {
            // Метод для відображення поточного стану системи
            public void DisplayStatus(VentilationSystem system)
            {
                Console.WriteLine("\n=== Стан системи вентиляції ===");
                foreach (var sensor in system.GetSensors())
                {
                    Console.WriteLine($"Датчик: {sensor.Type}, Значення: {sensor.CurrentValue}, Статус: {(sensor.IsOperational ? "Нормально" : "Несправний")}");
                }

                foreach (var actuator in system.GetActuators())
                {
                    Console.WriteLine($"Механізм: {actuator.Type}, Потужність: {actuator.CurrentLevel}%, Статус: {(actuator.IsActive ? "Увімкнено" : "Вимкнено")}");
                }
            }

            // Метод для виведення повідомлень про помилки
            public void ShowAlert(string message)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ПОМИЛКА] {message}");
                Console.ResetColor();
            }

            // Метод для запису подій
            public void LogEvent(string message)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[ПОДІЯ] {message}");
                Console.ResetColor();
            }
        }
    }

namespace SCADA
    {
        // Клас для обробки помилок у системі
        public class ErrorHandler
        {
            // Метод для обробки конкретної помилки
            public void HandleError(string errorType, string details)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n=== ПОМИЛКА ===\nТип: {errorType}\nДеталі: {details}\n");
                Console.ResetColor();

                // Обробка різних типів помилок
                switch (errorType)
                {
                    case "OutOfRange":
                        Console.WriteLine("Дія: Перевірте датчики. Калібруйте або замініть несправний датчик.");
                        break;
                    case "ActuatorFailure":
                        Console.WriteLine("Дія: Перевірте виконавчий механізм. Можливо, він потребує ремонту.");
                        break;
                    default:
                        Console.WriteLine("Дія: Зверніться до технічного персоналу.");
                        break;
                }
            }
        }
    }


namespace SCADA
    {
        // Клас для запису логів у файл
        public class Logger
        {
            private string logFilePath;

            public Logger(string path = "system_log.txt")
            {
                logFilePath = path;
            }

            // Метод для запису повідомлення в лог
            public void WriteLog(string message)
            {
                string logMessage = $"[{DateTime.Now}] {message}";
                Console.WriteLine(logMessage); // Для відображення в консолі
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine); // Запис у файл
            }
        }
    }


namespace SCADA
    {
        public class VentilationSystem
        {
            private List<Sensor> sensors = new List<Sensor>();
            private List<Actuator> actuators = new List<Actuator>();
            private Controller controller = new Controller();
            private ErrorHandler errorHandler = new ErrorHandler();
            private Logger logger = new Logger();

            // Додавання датчика
            public void AddSensor(Sensor sensor)
            {
                sensors.Add(sensor);
                logger.WriteLog($"Додано датчик: {sensor.Type}");
            }

            // Додавання виконавчого механізму
            public void AddActuator(Actuator actuator)
            {
                actuators.Add(actuator);
                logger.WriteLog($"Додано механізм: {actuator.Type}");
            }

            // Запуск системи
            public void StartSystem()
            {
                Console.WriteLine("Запуск системи вентиляції...");
                foreach (var actuator in actuators)
                {
                    actuator.Start();
                    logger.WriteLog($"Механізм {actuator.Type} увімкнено.");
                }
            }

            // Зупинка системи
            public void StopSystem()
            {
                Console.WriteLine("Зупинка системи вентиляції...");
                foreach (var actuator in actuators)
                {
                    actuator.Stop();
                    logger.WriteLog($"Механізм {actuator.Type} вимкнено.");
                }
            }

            // Моніторинг системи
            public void MonitorSystem(double targetTemperature, double targetHumidity, SCADAInterface ui)
            {
                foreach (var sensor in sensors)
                {
                    sensor.ReadValue();
                    logger.WriteLog($"Датчик {sensor.Type} зчитав значення: {sensor.CurrentValue}");

                    if (controller.CheckForErrors(sensor))
                    {
                        errorHandler.HandleError("OutOfRange", $"Датчик {sensor.Type} вийшов за межі.");
                        ui.ShowAlert($"Датчик {sensor.Type} вийшов за допустимі межі!");
                    }
                }

                // Приклад: управління нагрівачем
                var tempSensor = sensors.Find(s => s.Type == "Temperature");
                var heater = actuators.Find(a => a.Type == "Heater");
                if (tempSensor != null && heater != null)
                {
                    double action = controller.CalculateAction(tempSensor.CurrentValue, targetTemperature, 1.0, 0.5);
                    heater.AdjustPower(action);
                    logger.WriteLog($"Регулювання потужності Heater до {action}%.");
                }
            }

            // Отримати список датчиків
            public List<Sensor> GetSensors()
            {
                return sensors;
            }

            // Отримати список виконавчих механізмів
            public List<Actuator> GetActuators()
            {
                return actuators;
            }
   

namespace SCADA
        {
            class Program
            {
                static void Main(string[] args)
                {
                    // Ініціалізація системи вентиляції
                    var system = new VentilationSystem();
                    var ui = new SCADAInterface();

                    // Додавання датчиків
                    system.AddSensor(new Sensor("Temperature", 18.0, 25.0));
                    system.AddSensor(new Sensor("Humidity", 30.0, 60.0));

                    // Додавання виконавчих механізмів
                    system.AddActuator(new Actuator("Heater"));
                    system.AddActuator(new Actuator("Cooler"));

                    // Запуск системи
                    system.StartSystem();

                    // Моніторинг параметрів
                    double targetTemperature = 22.0;
                    double targetHumidity = 50.0;

                    Console.WriteLine("Моніторинг системи...");
                    for (int i = 0; i < 5; i++) // Імітація 5 ітерацій моніторингу
                    {
                        system.MonitorSystem(targetTemperature, targetHumidity, ui);
                        ui.DisplayStatus(system);
                        System.Threading.Thread.Sleep(2000); // Затримка між циклами
                    }

                    // Зупинка системи
                    system.StopSystem();
                    Console.WriteLine("Система завершила роботу.");
                }
            }

        }

    }
