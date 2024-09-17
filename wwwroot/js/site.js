function show_hide_password(target) {
    const input = document.getElementById('password-input');
    if (input.getAttribute('type') === 'password') {
        target.classList.add('view');
        input.setAttribute('type', 'text');
    } else {
        target.classList.remove('view');
        input.setAttribute('type', 'password');
    }
    return false;
}

function show_hide_password_confirm(target) {
    const input = document.getElementById('password-input_confirm');
    if (input.getAttribute('type') === 'password') {
        target.classList.add('view');
        input.setAttribute('type', 'text');
    } else {
        target.classList.remove('view');
        input.setAttribute('type', 'password');
    }
    return false;
}

let savedItems = JSON.parse(localStorage.getItem('savedItems')) || [];

document.addEventListener('DOMContentLoaded', function () {
    const saveButtons = document.querySelectorAll('.save-button');

    saveButtons.forEach(button => {
        const itemId = button.getAttribute('data-id');
        if (savedItems.includes(itemId)) {
            button.classList.remove('btn-outline-success');
            button.classList.add('btn-success');
            button.textContent = 'Сохранено';
        }
        
        button.addEventListener('click', function () {
            const form = this.closest('form'); // Находим соответствующую форму
            const formData = new FormData(form);
            // Проверяем, сохранён ли товар
            if (savedItems.includes(itemId)) {
                // Убираем из сохранённых
                savedItems = savedItems.filter(id => id !== itemId);

                // Изменяем класс кнопки обратно
                button.classList.remove('btn-success');
                button.classList.add('btn-outline-success');
                button.textContent = 'Сохранить';

                // Обновляем localStorage
                localStorage.setItem('savedItems', JSON.stringify(savedItems));

                // Можно добавить логику удаления с сервера, если это необходимо
                fetch(form.action, {
                    method: form.method,
                    body: formData
                })
                    .then(response => {
                        if (response.ok) {
                            console.log('Товар успешно убран.');
                        } else {
                            console.log('Ошибка при удаление сохранения товара.');
                        }
                    })
                    .catch(error => console.error('Ошибка:', error));
            } 
            else {
                // Если товар не сохранён, сохраняем его
                savedItems.push(itemId);

                // Изменяем класс кнопки
                button.classList.remove('btn-outline-success');
                button.classList.add('btn-success');
                button.textContent = 'Сохранено';

                // Обновляем localStorage
                localStorage.setItem('savedItems', JSON.stringify(savedItems));

                // Отправляем запрос на сервер
                fetch(form.action, {
                    method: form.method,
                    body: formData
                })
                    .then(response => {
                        if (response.ok) {
                            console.log('Товар успешно сохранён.');
                        } else {
                            console.log('Ошибка при сохранении товара.');
                        }
                    })
                    .catch(error => console.error('Ошибка:', error));
            }
        });
    });
});
