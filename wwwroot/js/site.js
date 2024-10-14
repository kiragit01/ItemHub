

const darkModeSwitchInput = document.querySelector('input#darkModeSwitch');
const bodyTag = document.querySelector('[data-tag="html"]');

// Функция для переключения темы
const themeSwitch = () => {
    const currentState = bodyTag.getAttribute('data-bs-theme');
    let newTheme = currentState === "light" ? "dark" : "light";

    // Устанавливаем новую тему на body
    bodyTag.setAttribute('data-bs-theme', newTheme);

    // Сохраняем выбранную тему в localStorage
    localStorage.setItem('theme', newTheme);
};

// Проверка сохраненной темы при загрузке страницы
const savedTheme = localStorage.getItem('theme');

if (savedTheme) {
    // Если тема сохранена в localStorage, применяем ее
    bodyTag.setAttribute('data-bs-theme', savedTheme);
    darkModeSwitchInput.checked = savedTheme === "dark";
} else {
    // Если тема не сохранена, проверяем системные настройки пользователя
    const prefersDarkScheme = window.matchMedia("(prefers-color-scheme: dark)").matches;

    // Устанавливаем тему в зависимости от системных настроек
    const systemTheme = prefersDarkScheme ? "dark" : "light";
    bodyTag.setAttribute('data-bs-theme', systemTheme);

    // Устанавливаем состояние переключателя в зависимости от системной темы
    darkModeSwitchInput.checked = prefersDarkScheme;
}

// Добавляем обработчик на переключатель темы
darkModeSwitchInput.addEventListener('change', themeSwitch);

