document.addEventListener("productsLoaded", function () {
    document.querySelectorAll(".published").forEach(function (button) {
        const card = button.closest(".row")
        const unpublished = '<div ' +
            'class="h1 position-absolute top-50 start-50 translate-middle w-auto" ' +
            'style="z-index: 1" id="unpublished">Скрыто</div>'        
        button.addEventListener("click", async function (e) {
            e.preventDefault();

            const id = button.dataset.id;

            try {
                const response = await fetch(`/api/items/${id}/publish`, {
                    method: "PUT",
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ id: id })
                });

                if (!response.ok) {
                    throw new Error("Ошибка выполнения запроса к серверу");
                }

                const result = await response.json();

                if (!result.isPublished) {
                    button.textContent = "Опубликовать";
                    card.style.cssText = "filter: opacity(0.4);";
                    card.insertAdjacentHTML( 'afterbegin', unpublished )
                } else {
                    button.textContent = "Скрыть";
                    card.style.cssText = "filter: opacity(1);";
                    card.removeChild(card.firstChild);
                }
            } catch (error) {
                console.error("Ошибка: ", error);
                alert("Произошла ошибка при изменении статуса товара.");
            }
        });
        if (button.textContent === "Опубликовать") {
            card.style.cssText = "filter: opacity(0.4);";
            card.insertAdjacentHTML( 'afterbegin', unpublished )
        }
    });
});
