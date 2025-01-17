let maxPriceGlobal = 10000; // по умолчанию. Потом получим реальное значение с сервера.
let isInitializing = true; // флаг, чтобы при первом запуске не было лишних doSearch(1)
let searchTimeout;
let searchInputTimeout;

document.addEventListener("DOMContentLoaded", function () {
    // Считываем из URL (если есть)
    const params = new URLSearchParams(window.location.search);
    const queryParam = params.get("query") || "";
    const pageParam = parseInt(params.get("page") || "1");
    const minP = parseInt(params.get("minPrice") || "0");
    const maxP = parseInt(params.get("maxPrice") || "10000");

    const sliderLeft = document.getElementById('slider-left');
    const sliderRight = document.getElementById('slider-right');
    const minPriceInput = document.getElementById('minPrice');
    const maxPriceInput = document.getElementById('maxPrice');
    const searchBtn = document.getElementById('searchBtn');
    const searchQuery = document.getElementById('searchQuery');
    
    minPriceInput.value = minP;
    maxPriceInput.value = maxP;
    searchQuery.value = queryParam;
    
    // Получаем максимальную цену (при условии, что у вас есть метод GetMaxPrice)
    fetch('/Home/GetMaxPrice')
        .then(response => {
            if (!response.ok) {
                throw new Error('Ошибка при получении максимальной цены');
            }
            return response.json();
        })
        .then(data => {
            maxPriceGlobal = data.maxPrice || 10000;
            initPriceRangeSlider(0, maxPriceGlobal);
            doSearch(pageParam);
            isInitializing = false;
        })
        .catch(error => {
            console.log('Ошибка получения максимальной цены:', error);
            initPriceRangeSlider(0, maxPriceGlobal);
            doSearch(pageParam);
            isInitializing = false;
        });
    
    // Подключаем обработчик popstate
    window.addEventListener("popstate", function(e) {
        if (e.state) {
            document.getElementById("searchQuery").value = e.state.query;
            document.getElementById("minPrice").value = e.state.minPrice;
            document.getElementById("maxPrice").value = e.state.maxPrice;
            doSearch(e.state.page, false); // false => не делаем pushState повторно
        }
    });
    
    // При клике на кнопку "Искать"
    searchBtn.addEventListener("click", function () {
        doSearch(1);
    });

    // Event listener for search input
    searchQuery.addEventListener('input', function() {
        clearTimeout(searchInputTimeout);
        searchInputTimeout = setTimeout(() => {
            doSearch(1);
        }, 1000);
    });

    searchQuery.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            clearTimeout(searchInputTimeout);
            doSearch(1);
        }
    });

    // Function to initialize the price range slider
    function initPriceRangeSlider(minValue, maxValue) {
        sliderLeft.min = sliderRight.min = minValue;
        sliderLeft.max = sliderRight.max = maxValue;
        sliderLeft.value = minP || minValue;
        sliderRight.value = maxP || maxValue;
        minPriceInput.value = minP || minValue;
        maxPriceInput.value = maxP || maxValue;

        updateSlider();
    }

    // Function to update the slider appearance and input values
    function updateSlider() {
        const leftValue = parseInt(sliderLeft.value);
        const rightValue = parseInt(sliderRight.value);
        const track = document.querySelector('.slider-track');

        if (leftValue > rightValue) {
            [sliderLeft.value, sliderRight.value] = [sliderRight.value, sliderLeft.value];
        }

        const leftPercent = (leftValue / maxPriceGlobal) * 100;
        const rightPercent = (rightValue / maxPriceGlobal) * 100;

        track.style.background = `linear-gradient(to right, #e9ecef ${leftPercent}%, #0d6efd ${leftPercent}%, #0d6efd ${rightPercent}%, #e9ecef ${rightPercent}%)`;

        minPriceInput.value = sliderLeft.value;
        maxPriceInput.value = sliderRight.value;

        if (!isInitializing) {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                // При движении ползунка начинаем с 1 страницы
                doSearch(1);
            }, 1000);
        }
    }

    // Event listeners for sliders
    sliderLeft.addEventListener('input', updateSlider);
    sliderRight.addEventListener('input', updateSlider);

    // Event listeners for input fields
    minPriceInput.addEventListener('change', function() {
        let val = parseInt(this.value);
        if (isNaN(val)) val = 0;
        if (val < 0) val = 0;
        if (val > parseInt(sliderRight.value)) val = parseInt(sliderRight.value);
        sliderLeft.value = val;
        updateSlider();
    });

    maxPriceInput.addEventListener('change', function() {
        let val = parseInt(this.value);
        if (isNaN(val)) val = maxPriceGlobal;
        if (val < parseInt(sliderLeft.value)) val = parseInt(sliderLeft.value);
        if (val > maxPriceGlobal) val = maxPriceGlobal;
        sliderRight.value = val;
        updateSlider();
    });
});

// AJAX-функция поиска
function doSearch(page, pushHistory = true) {
    let query = document.getElementById("searchQuery").value || "";
    let minPrice = parseInt(document.getElementById("minPrice").value) || 0;
    let maxPrice = parseInt(document.getElementById("maxPrice").value) || maxPriceGlobal;

    // Подготавливаем URL
    let url = `/Home/SearchItemsAjax?query=${encodeURIComponent(query)}&minPrice=${minPrice}&maxPrice=${maxPrice}&page=${page}`;
    // Если это страница MyItems, добавим onlyMine=true
    if (window.IS_MYITEMS_PAGE === true) {
        url += "&onlyMine=true";
    }
    fetch(url)
        .then(resp => resp.text())
        .then(html => {
            document.getElementById("search-results").innerHTML = html;
            bindPaginationLinks();

            if (pushHistory) {
                // Собираем query-параметры УРЛа (для адресной строки):
                let newUrlParts = [];

                if (query) {
                    newUrlParts.push(`query=${encodeURIComponent(query)}`);
                }
                if (minPrice !== 0) {
                    newUrlParts.push(`minPrice=${minPrice}`);
                }
                if (maxPrice !== maxPriceGlobal) {
                    newUrlParts.push(`maxPrice=${maxPrice}`);
                }
                if (page !== 1) {
                    newUrlParts.push(`page=${page}`);
                }

                let newUrl = "";
                if (newUrlParts.length > 0) {
                    newUrl = "?" + newUrlParts.join("&");
                } else {
                    // Ничего не меняли, остаёмся на /Home/Index без query string
                    // Или используем window.location.pathname, если надо
                    newUrl = window.location.pathname;
                    // Например, /Home/Index или "/"
                }

                window.history.pushState({ page, query, minPrice, maxPrice }, "", newUrl);
            }
        })
        .catch(e => {
            console.error(e);
            document.getElementById("search-results").innerHTML = `<p class="text-danger">Ошибка загрузки результатов</p>`;
        });
}

// Перехват кликов на пагинаторе
function bindPaginationLinks() {
    let links = document.querySelectorAll("#search-results .page-link");
    links.forEach(link => {
        link.addEventListener("click", function (e) {
            // если ссылка ведет на page=? -> делаем AJAX
            let href = link.getAttribute("href");
            if (!href) return;
            // предотвратить переход
            e.preventDefault();

            // Парсим query-параметр "page"
            let match = href.match(/page=(\d+)/);
            let pageNumber = match ? parseInt(match[1]) : 1;

            doSearch(pageNumber);
        });
    });
}

