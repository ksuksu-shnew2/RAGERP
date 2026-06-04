mp.events.add("playerReady", () => {
    mp.gui.chat.push("Добро пожаловать на сервер!");
});
mp.events.add("playerJoinedServer", (name) => {
    mp.gui.chat.push(`~g~[+]~w~ ${name} зашёл на сервер!`);
});
