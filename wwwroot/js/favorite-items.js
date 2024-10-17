document.addEventListener('DOMContentLoaded', function () {
    const saveButtons = document.querySelectorAll('.save-button');
    const favoriteCountElement = document.getElementById('count');

    // Функция для обновления количества сохранённых товаров с сервера
    function updateFavoriteCount() {
        fetch('/favcount', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(response => response.json())
            .then(count => {
                favoriteCountElement.textContent = count;
            })
            .catch(error => console.error('Ошибка при получении количества сохранённых товаров:', error));
    }

    // Функция для обновления состояния кнопок сохранения
    function updateSaveButtons() {
        fetch('/GetFavoritedItems', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(response => response.json())
            .then(favoritedItems => {
                saveButtons.forEach(button => {
                    const itemId = button.getAttribute('data-id');
                    if (favoritedItems.includes(itemId)) {
                        button.classList.remove('btn-outline-success');
                        button.classList.add('btn-success');
                        button.textContent = 'Сохранено';
                    } else {
                        button.classList.remove('btn-success');
                        button.classList.add('btn-outline-success');
                        button.textContent = 'Сохранить';
                    }
                });
            })
            .catch(error => console.error('Ошибка при обновлении кнопок сохранения:', error));
    }

    // Обновляем количество и состояние кнопок при загрузке страницы
    updateFavoriteCount();
    updateSaveButtons();

    saveButtons.forEach(button => {
        const itemId = button.getAttribute('data-id');

        button.addEventListener('click', function () {
            fetch('/FavoritedItems', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ id: itemId })
            })
                .then(response => {
                    if (response.ok) {
                        // Обновляем состояние кнопок и количество сохранённых товаров после изменения
                        updateSaveButtons();
                        updateFavoriteCount();
                    } else {
                        console.log('Ошибка при сохранении/удалении товара.');
                    }
                })
                .catch(error => console.error('Ошибка:', error));
        });
    });
});