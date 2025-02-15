const favoriteCountElement = document.getElementById('count');

const savedFavorites = localStorage.getItem('favorites');
if (savedFavorites) {
    try {
        favoriteCountElement.textContent = JSON.parse(savedFavorites).length;
    } catch (error) {
        console.error('Ошибка парсинга favorites из localStorage:', error);
    }
}