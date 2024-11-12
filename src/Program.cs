using System.Collections.Generic;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var token = Environment.GetEnvironmentVariable("YOUR_BOT_TOKEN") ?? "8007983985:AAF3MRtvWfHoSFvujVy2KXCQwl1ZTDWV5QA";

Dictionary<long, string> userNames = new Dictionary<long, string>();
Dictionary<long, string> userBets = new Dictionary<long, string>();

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
    Console.WriteLine(exception);
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
        switch (msg.Text)
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
                        InlineKeyboardButton.WithCallbackData("А это просто кнопка", "button1"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Тут еще одна", "button2"),
                        InlineKeyboardButton.WithUrl("О том, как создать такого же Бота (Ну или другого)", "https://telegrambots.github.io/book/1/quickstart.html"),
                    },
                });

                await bot.SendMessage(chatId, "Добро пожаловать на вкладку Информации!", replyMarkup: inlineKeyboard);
                break;

            case "/Bets":
                var replyKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    new[]
                    {
                        new KeyboardButton("Сделать Ставку!"),
                        new KeyboardButton("Текущие Ставки!"),
                    },
                    new[]
                    {
                        new KeyboardButton("Результат Ставок!"),
                        new KeyboardButton("Таблица Лидеров!"),
                    }
                })
                {
                    ResizeKeyboard = true,
                };

                await bot.SendMessage(chatId, "Это вкладка по Ставкам!\nРядом со строкой ввода Сообщений, у вас появилась - Загадочная Кнопка - Нажмите её!", replyMarkup: replyKeyboard);
                break;

            default:
                await bot.SendMessage(chatId, $"Неизвестная команда, {userNames[chatId]}. Используйте /start для начала.");
                break;
        }
    }
}

async Task ShowMainMenu(long chatId)
{
    await bot.SendMessage(chatId, $"Добро пожаловать, {userNames[chatId]}! Выберите Категорию:\n" +
        "/Information\n" +
        "/Bets\n");
}

async Task OnUpdate(Update update)
{
    if (update.CallbackQuery != null)
    {
        await bot.AnswerCallbackQuery(update.CallbackQuery.Id, $"Вы выбрали {update.CallbackQuery.Data}");
        await bot.SendMessage(update.CallbackQuery.Message.Chat.Id, $"Пользователь {update.CallbackQuery.From} нажал на {update.CallbackQuery.Data}");
    }
    else if (update.Message != null && update.Message.Type == MessageType.Text)
    {
        long chatId = update.Message.Chat.Id;

        switch (update.Message.Text)
        {
            case "Сделать Ставку!":
                var betOptionsKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    new[]
                    {
                        new KeyboardButton("Ставка на команду NaVi"),
                        new KeyboardButton("Ставка на команду VirtusPro"),
                    },
                    new[]
                    {
                        new KeyboardButton("Назад"),
                    }
                })
                {
                    ResizeKeyboard = true,
                };

                await bot.SendMessage(chatId, "Выберите, на какую команду вы хотите сделать ставку:", replyMarkup: betOptionsKeyboard);
                break;

            case "Текущие Ставки!":
                if (userBets.TryGetValue(chatId, out string userBet))
                {
                    await bot.SendMessage(chatId, $"Ваша текущая ставка: {userBet}");
                }
                else
                {
                    await bot.SendMessage(chatId, "Ставок нет.");
                }
                break;

            case "Ставка на команду NaVi":
            case "Ставка на команду VirtusPro":
                userBets[chatId] = update.Message.Text;
                await bot.SendMessage(chatId, $"Вы сделали ставку на: {update.Message.Text}");
                break;

            case "Назад":
                await ShowMainMenu(chatId);
                break;

            default:
                await bot.SendMessage(chatId, $"Неизвестная команда, {userNames[chatId]}. Используйте /start для начала.");
                break;
        }
    }
}







// 8007983985:AAF3MRtvWfHoSFvujVy2KXCQwl1ZTDWV5QA

/*
            await bot.SendMessage(msg.Chat, """
                <b><u>Меню</u></b>:
                /описание - расскажем, о чем вообще это дело
                /где      - а где собственно мы засели?
                /услуги   - что мы можем сделать
                /контакты - кому звонить в случае чего
                """, parseMode: ParseMode.Html, linkPreviewOptions: true);
            break;




    async Task OnMessage(Message msg, UpdateType type)
{
    if (n == 1)
    {
        await bot.SendMessage(msg.Chat, "Здравствуйте! Как вас зовут?");
        n++;
        j++;
    }
    else if (n == 2)
    {
        string userName = msg.Text;
        nameChat.Add(i, userName);
        string UserName = nameChat[1];
        var nameExists1 = nameChat.ContainsKey(1);
        await bot.SendMessage(msg.Chat, "Приятно познакомиться, " + UserName + "!");
        await bot.SendMessage(msg.Chat, "Добро Пожаловать на Бота по Ставкам КС2!\n" +
            "Пожалуйста, выберите Клавиатуру: ");
        n++;
        {
            if (msg.Text is not { } text)
                Console.WriteLine($"Received a message of type {msg.Type}");
            else if (text.StartsWith('/'))
            {
                var space = text.IndexOf(' ');
                if (space < 0) space = text.Length;
                var command = text[..space].ToLower();
                if (command.LastIndexOf('@') is > 0 and int at)
                    if (command[(at + 1)..].Equals(me.Username, StringComparison.OrdinalIgnoreCase))
                        command = command[..at];
                    else
                        return;
                await OnCommand(command, text[space..].TrimStart(), msg);
            }
            else
                await OnTextMessage(msg);
        }
    }
    else
    {
        {
            if (msg.Text is not { } text)
                Console.WriteLine($"Received a message of type {msg.Type}");
            else if (text.StartsWith('/'))
            {
                var space = text.IndexOf(' ');
                if (space < 0) space = text.Length;
                var command = text[..space].ToLower();
                if (command.LastIndexOf('@') is > 0 and int at)
                    if (command[(at + 1)..].Equals(me.Username, StringComparison.OrdinalIgnoreCase))
                        command = command[..at];
                    else
                        return;
                await OnCommand(command, text[space..].TrimStart(), msg);
            }
            else
                await OnTextMessage(msg);
        }
    }
}
async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query })
    {
        await bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
        await bot.SendMessage(query.Message!.Chat, $"User {nameChat[1]} clicked on {query.Data}");
    }
}
async Task OnTextMessage(Message msg)
{
    Console.WriteLine($"Received text '{msg.Text}' in {msg.Chat}");
    await OnCommand("/start", "", msg);
}

async Task OnCommand(string command, string args, Message msg)
{
    Console.WriteLine($"Received command: {command} {args}");
    switch (command)
    {
        case "/start":
            await bot.SendTextMessageAsync(chatId, $"Добро пожаловать, {userNames[chatId]}! Выберите клавиатуру:\n" +
                "/ставки\n" +
                "/reply\n");
            break;
        case "/описание":
            await bot.SendMessage(msg.Chat, """
                Автомастерская <b><u>'Шота у Ашота'</u></b>
                Понтов мало, много дела в отличии от распальцованных сервисов
                Работаем над любыми тачками, забугорными и нашими
                Качество выше чем горы Кавказа!
                """, parseMode: ParseMode.Html, linkPreviewOptions: true);
            break;
        case "/где":
            await bot.SendMessage(msg.Chat, "Наша точка находится на" +
                "\nШипиловская ул., 28, Москва, 115569 " +
                "\nПриезжай, " + nameChat[1] + ", мы тебя ждем!" +
                "\nРаботаем от зари до зари!");
            break;
        case "/контакты":
            await bot.SendMessage(msg.Chat, "Как с нами созвониться:" +
                "\nАшот Танкян - 8 (495) XXX-XX-XX" +
                "\n- +7(901) 365-27-XX" +
                "\nД. Малакян - 8 (495) ZZZ-ZZ-ZZ" +
                "\nПочта: manilovecars@mail.ru");
            break;
        case "/то":
            await bot.SendMessage(msg.Chat, "Техобслуживание::Замена масла - 800,00 ₽" +
                "\nТехобслуживание::Развал схождения - 1 500,00 ₽" +
                "\nТехобслуживание::Замена свечей - 800,00 ₽" +
                "\nТехобслуживание::Компьютерная диагностика - 1 200,00 ₽" +
                "\nТехобслуживание::Диагностика двигателя");
            break;
        case "/диагноз":
            await bot.SendMessage(msg.Chat, "Диагностика::Компьютерная диагностика - 800,00 ₽" +
                "\nДиагностика::Диагностика подвески - 800,00 ₽" +
                "\nДиагностика::Система зажигания - 800,00 ₽" +
                "\nДиагностика::Система топливоподачи и система впрыска бензиновых двигателей -  1 500,00 ₽");
            break;
        case "/мойка":
            await bot.SendMessage(msg.Chat, "Автомойка::Химчистка салона - 4 500,00 ₽" +
                "\nАвтомойка::Дезинфекция автомобиля - 300,00 ₽" +
                "\nАвтомойка::Комплексная автомойка премиум класса - 9 500, 00₽" +
                "\nАвтомойка::Полировка кузова и фар - 500,00 ₽");
            break;
        case "/двигатель":
            await bot.SendMessage(msg.Chat, "Двигатель::Промывка инжектора и форсунок - 2 000,00 ₽" +
                "\nДвигатель::Замена масла - 800,00 ₽" +
                "\nДвигатель::Замена свечей - 600,00 ₽" +
                "\nДвигатель::Капитальный ремонт двигателей - по случаю");
            break;
        case "/шиномонтаж":
            await bot.SendMessage(msg.Chat, "Шиномонтаж - 1 000,00 ₽" +
                "\nШиномонтаж::Балансировка - 1 000,00 ₽" +
                "\nШиномонтаж::Ремонт дисков - 1 000,00 ₽");
            break;
        case "/трансмиссия":
            await bot.SendMessage(msg.Chat, "Трансмиссия::Замена масла в мостах - 1 000,00 ₽" +
                "\nТрансмиссия::Замена шрусов - 1 000,00 ₽" +
                "\nТрансмиссия::Замена сцепления - 3 000,00 ₽");
            break;
        case "/покраска":
            await bot.SendMessage(msg.Chat, ":Покраска кузова - 5 900,00 ₽ ₽" +
                "\nПодбор краски - 1 000,00 ₽" +
                "\nУстранение царапин - 4 000,00 ₽");
            break;
        case "/услуги":
            await bot.SendMessage(msg.Chat, """
                Мы выполняем кучу всего разного!
                Что интересует?
                <b><u>Меню</u></b>:
                /то          - техобслуживание
                /диагноз     - диагностика, посмотрим что с тачкой не так
                /мойка       - машинка будет блестеть как пик Эльбруса
                /двигатель   - заглянем под капот
                /трансмиссия - чтоб не улетел в кювет
                /шиномонтаж  - подготовим к зиме
                /покраска    - закрасим любые царапины
                """, parseMode: ParseMode.Html, linkPreviewOptions: true);
            break;
    }
}
*/

// 8007983985:AAF3MRtvWfHoSFvujVy2KXCQwl1ZTDWV5QA
