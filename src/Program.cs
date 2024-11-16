using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var token = Environment.GetEnvironmentVariable("YOUR_BOT_TOKEN") ?? "...";

Dictionary<long, string> userNames = new Dictionary<long, string>();
Dictionary<long, List<string>> userBets = new Dictionary<long, List<string>>();
Dictionary<long, List<string>> leaderBoard = new Dictionary<long, List<string>>();

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var me = await bot.GetMe();

bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"@{me.Username} is running... Press Escape to terminate");
while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
cts.Cancel();

async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine($"Error: {exception.Message}");
    await Task.Delay(2000, cts.Token);
}

async Task OnMessage(Message msg, UpdateType type)
{
    long chatId = msg.Chat.Id;

    if (!userNames.ContainsKey(chatId))
    {
        await bot.SendMessage(chatId, "Приветствую! Как вас зовут?");
        userNames[chatId] = "";
        return;
    }

    if (string.IsNullOrEmpty(userNames[chatId]))
    {
        userNames[chatId] = msg.Text;
        await bot.SendMessage(chatId, $"Приятно познакомиться, {userNames[chatId]}!\nДобро пожаловать на Бота по Ставкам КС2!");
        await ShowMainMenu(chatId);
        return;
    }

    if (msg.Type == MessageType.Text)
    {
        await HandleTextMessage(chatId, msg.Text);
    }
}

async Task HandleTextMessage(long chatId, string text)
{
    Console.WriteLine($"Received command: '{text}' from chatId: {chatId}");

    var Welcomes = new[] { "привет", "здравствуй", "добрый день", "доброе утро", "добрый вечер" };

    if (Welcomes.Contains(text.Trim().ToLower()))
    {
        await bot.SendMessage(chatId, $"Привет, {userNames[chatId]}!");
        return;
    }

    switch (text.Trim())
    {
        case "/start":
            await ShowMainMenu(chatId);
            break;

        case "/Information":
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl("Сайт по CS2", "https://www.counter-strike.net/cs2"),
                    InlineKeyboardButton.WithCallbackData("Текущее Время", "current_time"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Текущая Дата", "current_date"),
                    InlineKeyboardButton.WithCallbackData("Изображение Великого", "send_image"),
                },
                 new[]
                {
                    InlineKeyboardButton.WithCallbackData("Видео", "send_video"),
                    InlineKeyboardButton.WithCallbackData("Стикер", "send_sticker"),
                },
                 new[]
                {
                    InlineKeyboardButton.WithUrl("Создание Бота", "https://telegrambots.github.io/book/1/example-bot.html"),
                },
            });

            await bot.SendMessage(chatId, "Добро пожаловать на вкладку Информации!", replyMarkup: inlineKeyboard);
            break;

        case "/Bets":
            var inlineKeyBoard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Сделать Ставку"),
                    InlineKeyboardButton.WithCallbackData("Мои Ставки"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Результат Ставки"),
                    InlineKeyboardButton.WithCallbackData("Таблица Лидеров"),
                }
            });

            await bot.SendMessage(chatId, "Это вкладка по Ставкам!\nВыберите действие:", replyMarkup: inlineKeyBoard);
            break;

        default:
            await bot.SendMessage(chatId, $"Извините, не знаю такой Команды, {userNames[chatId]}. Используйте /start для начала!");
            break;
    }
}

async Task ShowMainMenu(long chatId)
{
    await bot.SendMessage(chatId, "Пожалуйста, Выберите Категорию:\n" +
        "/Information\n" +
        "/Bets\n");
}

async Task OnUpdate(Update update)
{
    long chatId = update.CallbackQuery?.Message.Chat.Id ?? update.Message.Chat.Id;

    if (update.CallbackQuery != null)
    {
        await bot.AnswerCallbackQuery(update.CallbackQuery.Id, $"Вы выбрали {update.CallbackQuery.Data}");

        switch (update.CallbackQuery.Data)
        {
            case "Сделать Ставку":
                var betOptionsKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("NaVi"),
                        InlineKeyboardButton.WithCallbackData("VirtusPro"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Назад"),
                    }
                });

                await bot.SendMessage(chatId, "Выберите, на какую команду вы хотите сделать ставку:", replyMarkup: betOptionsKeyboard);
                break;

            case "NaVi":
                if (!userBets.ContainsKey(chatId))
                {
                    userBets[chatId] = new List<string>();
                }

                userBets[chatId].Add("NaVi");
                await bot.SendMessage(chatId, $"Вы сделали ставку на NaVi!");
                break;

            case "VirtusPro":
                if (!userBets.ContainsKey(chatId))
                {
                    userBets[chatId] = new List<string>();
                }

                userBets[chatId].Add("VirtusPro");
                await bot.SendMessage(chatId, $"Вы сделали ставку на VirtusPro!");
                break;

            case "Мои Ставки":
                if (userBets.ContainsKey(chatId) && userBets[chatId].Any())
                {
                    await bot.SendMessage(chatId, $"Ваши ставки: {string.Join(", ", userBets[chatId])}");
                }
                else
                {
                    await bot.SendMessage(chatId, "У вас пока нет ставок.");
                }
                break;

            case "Результат Ставки":
                if (userBets.ContainsKey(chatId) && userBets[chatId].Any())
                {
                    var random = new Random();
                    bool isWin = random.Next(0, 2) == 0;
                    var resultMessage = isWin ? "Поздравляем, ваша Ставка Сыграла. Больше не приходите! >:(" : "К Сожалению, ваша Ставка не сыграла. Приходите ещё!";
                    await bot.SendMessage(chatId, $"Результат вашей ставки: {resultMessage}");

                    if (isWin)
                    {
                        if (!leaderBoard.ContainsKey(chatId))
                        {
                            leaderBoard[chatId] = new List<string>();
                        }
                        leaderBoard[chatId].Add(userBets[chatId].Last());
                    }

                    userBets[chatId].Clear();
                }
                else
                {
                    await bot.SendMessage(chatId, "У вас нет ставок, чтобы проверить результат.");
                }
                break;

            case "Таблица Лидеров":
                if (leaderBoard.Any())
                {
                    var leaderMessage = "Таблица Лидеров:\n";
                    foreach (var entry in leaderBoard)
                    {
                        leaderMessage += $"{userNames[entry.Key]}: {string.Join(", ", entry.Value)}\n";
                    }
                    await bot.SendMessage(chatId, leaderMessage);
                }
                else
                {
                    await bot.SendMessage(chatId, "Таблица лидеров пуста.");
                }
                break;

            case "current_time":
                var currentTime = DateTime.Now.ToString("HH:mm:ss");
                await bot.SendMessage(chatId, $"🕒 Текущее Время: {currentTime}");
                break;

            case "current_date":
                var currentDate = DateTime.Now.ToString("dd MMMM yyyy");
                await bot.SendMessage(chatId, $"📅 Текущая Дата: {currentDate}");
                break;

            case "send_image":
                var message = await bot.SendPhoto(chatId, "https://s0.rbk.ru/v6_top_pics/media/img/9/35/755018868114359.jpg",
                    "<b>Великий!</b>. <i>Ресурс</i>: <a href=\"https://ya.ru/images/\">Яндекс</a>", ParseMode.Html);
                break;

            case "send_video":
                await bot.SendVideo(chatId, "https://telegrambots.github.io/book/docs/video-countdown.mp4",
                thumbnail: "https://telegrambots.github.io/book/2/docs/thumb-clock.jpg", supportsStreaming: true);
                break;

            case "send_sticker":
                var message1 = await bot.SendSticker(chatId, "https://telegrambots.github.io/book/docs/sticker-fred.webp");
                break;
            default:
                await bot.SendMessage(chatId, $"Неизвестная команда, {userNames[chatId]}. Используйте /start для начала.");
                break;
        }
    }
    else if (update.Message != null && update.Message.Type == MessageType.Text)
    {
        await HandleTextMessage(chatId, update.Message.Text);
    }
}