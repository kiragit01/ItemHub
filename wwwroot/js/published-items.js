document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".published").forEach(function (button) {
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

                if (result) {
                    button.textContent = "Опубликовать";
                    button.closest(".row").style.cssText = "filter: opacity(0.4);";
                } else {
                    button.textContent = "Скрыть";
                    button.closest(".row").style.cssText = "filter: opacity(1);";
                }
            } catch (error) {
                console.error("Ошибка: ", error);
                alert("Произошла ошибка при изменении статуса товара.");
            }
        });
        if (button.textContent === "Опубликовать") {
            button.closest(".row").style.cssText = "filter: opacity(0.4);";
        }
    });
});
