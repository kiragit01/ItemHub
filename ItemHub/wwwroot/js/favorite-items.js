document.addEventListener('productsLoaded', function () {
    const saveButtons = document.querySelectorAll('.save-button');
    const favoriteCountElement = document.getElementById('count');

    // Храним идентификаторы избранных товаров в Set для исключения дубликатов
    let favorites = new Set();

    // Инициализация favorites из localStorage (если есть сохранённые данные)
    const savedFavorites = localStorage.getItem('favorites');
    if (savedFavorites) {
        try {
            JSON.parse(savedFavorites).forEach(id => favorites.add(id));
        } catch (error) {
            console.error('Ошибка парсинга favorites из localStorage:', error);
        }
    }

    /**
     * Обновляет внешний вид кнопок сохранения в зависимости от состояния favorites.
     */
    function updateButtonsUI() {
        saveButtons.forEach(button => {
            const itemId = button.getAttribute('data-id');
            if (favorites.has(itemId)) {
                button.classList.remove('btn-outline-success');
                button.classList.add('btn-success');
                button.textContent = 'Сохранено';
            } else {
                button.classList.remove('btn-success');
                button.classList.add('btn-outline-success');
                button.textContent = 'Сохранить';
            }
        });
    }

    /**
     * Обновляет счётчик избранных товаров.
     */
    function updateFavoriteCount() {
        favoriteCountElement.textContent = favorites.size;
    }

    /**
     * Отправляет на сервер текущий список избранных товаров.
     * Ожидается, что серверный эндпоинт принимает JSON:
     * { Favorites: [ 'id1', 'id2', ... ] }
     */
    function flushFavorites() {
        // Если нечего отправлять, можно и не делать запрос
        const favoritesArray = Array.from(favorites);
        if (!favoritesArray.length) return;

        const payload = { Favorites: favoritesArray };

        fetch('/api/items/favorites/batch', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
            .then(response => {
                if (!response.ok) {
                    console.error('Ошибка при обновлении избранных товаров.');
                }
            })
            .catch(error => console.error('Ошибка при отправке избранных товаров:', error));
    }

    // Дебаунс: таймер, который сбрасывается при каждом клике
    let flushTimer = null;

    /**
     * Устанавливает таймер для отложенной отправки данных.
     * Если уже есть активный таймер, он сбрасывается.
     */
    function scheduleFlush() {
        if (flushTimer) clearTimeout(flushTimer);
        flushTimer = setTimeout(() => {
            flushFavorites();
            flushTimer = null;
        }, 5000);
    }

    // Обработка кликов по кнопкам сохранения
    saveButtons.forEach(button => {
        button.addEventListener('click', function () {
            const itemId = button.getAttribute('data-id');
            // Переключаем состояние: добавляем или удаляем идентификатор
            if (favorites.has(itemId)) {
                favorites.delete(itemId);
            } else {
                favorites.add(itemId);
            }
            // Сохраняем актуальный список в localStorage
            localStorage.setItem('favorites', JSON.stringify(Array.from(favorites)));
            updateButtonsUI();
            updateFavoriteCount();

            // Запускаем отложенную отправку
            scheduleFlush();
        });
    });

    // Если мы на странице, где отображаются избранные товары, сразу отправляем данные
    if (window.location.pathname.toLowerCase() === '/favorite') {
        flushFavorites();
    }

    // Инициализация UI при загрузке страницы
    updateButtonsUI();
    updateFavoriteCount();
});
