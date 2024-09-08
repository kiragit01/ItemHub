let queuedImagesArray = [],  // Новые изображения
    savedImagesArray = [],   // Сохраненные изображения
    displayDiv = document.querySelector(".queued-div"), // Общая зона отображения изображений
    inputDiv = document.querySelector(".input-div"),     // Зона сброса файлов
    input = document.querySelector(".input-div input"),
    serverMessage = document.querySelector(".server-message"),
    userLogin = document.querySelector(".user-login"),
    submitForm = document.querySelector("#submit1"),
    itemId = document.querySelector("#itemid");

// Загрузка сохранённых изображений при загрузке страницы
document.addEventListener("DOMContentLoaded", () => {
    fetch("/get-saved-images?login=" + userLogin.textContent + `&id=${itemId.value}`)  // Запрос на получение уже загруженных изображений
        .then(response => response.json())
        .then(data => {
            savedImagesArray = data;
            displayImages();   // Отображаем уже загруженные изображения
        });
});

// Обработка события выбора файлов через input
input.addEventListener("change", () => {
    const files = input.files;
    for (let i = 0; i < files.length; i++) {
        queuedImagesArray.push(files[i]);
    }
    input.value = "";  // Очистка input после добавления файлов
    displayImages();   // Обновляем отображение
});

// Обработка drag and drop для input
input.addEventListener("dragenter", function () {
    inputDiv.classList.add("_active");
});

input.addEventListener("dragleave", function () {
    inputDiv.classList.remove("_active");
});

inputDiv.addEventListener("drop", (e) => {
    inputDiv.classList.remove("_active");
    e.preventDefault();
    const files = e.dataTransfer.files;
    for (let i = 0; i < files.length; i++) {
        if (!files[i].type.match("image")) continue;
        if (queuedImagesArray.every(image => image.name !== files[i].name))
            queuedImagesArray.push(files[i]);
    }
    displayImages();  // Обновляем отображение
});

// Функция для отображения всех изображений (и сохранённых, и новых)
function displayImages() {
    let images = "";

    // Отображаем сохраненные изображения
    savedImagesArray.forEach((image, index) => {
        images += `<div class="image">
                        <img src="/images/${userLogin.textContent}/${itemId.value}/${image.fileName}" alt="saved image"> 
                        <span onclick="deleteSavedImage(${index})">&times;</span>
                   </div>`;
    });

    // Отображаем новые изображения
    queuedImagesArray.forEach((image, index) => {
        images += `<div class="image">
                        <img src="${URL.createObjectURL(image)}" alt="queued image"> 
                        <span onclick="deleteQueuedImage(${index})">&times;</span>
                   </div>`;
    });

    displayDiv.innerHTML = images;
}

// Удаление нового изображения
function deleteQueuedImage(index) {
    queuedImagesArray.splice(index, 1);
    displayImages();
}

// Удаление сохраненного изображения
function deleteSavedImage(index) {
    const imageName = savedImagesArray[index].fileName;

    fetch(`/delete-image?login=${userLogin.textContent}&id=${itemId.value}&fileName=${imageName}`, {
        method: 'DELETE'
    }).then(response => {
        if (response.ok) {
            savedImagesArray.splice(index, 1);  // Удаляем из списка
            displayImages();  // Обновляем отображение
        }
    });
}

// Обработка отправки формы
submitForm.addEventListener("submit", (e) => {
    e.preventDefault();

    const formData = new FormData(submitForm);

    // Добавляем изображения в FormData
    savedImagesArray.forEach((image, index) => {
        formData.append(`savedImages[]`, `/images/${userLogin.textContent}/${itemId.value}/${image.fileName}`);  // Передаем только имя файла
    });

    // Добавляем новые изображения в FormData
    queuedImagesArray.forEach((image, index) => {
        formData.append(`Images`, image);  // Передаем сам файл
    });

    // Отправка данных через fetch
    fetch("/edit", {
        method: "POST",
        body: formData
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Ошибка при обновлении");
            }
            return response.text(); // Или JSON, в зависимости от ответа сервера
        })
        .then(data => {
            // Обработка успешного ответа
            console.log("Успешно обновлено", data);
            location.replace(`/item?id=${itemId.value}`);
        })
        .catch(error => {
            serverMessage.innerHTML = error;
            serverMessage.style.cssText = "background-color: #f8d7da; color: #b71c1c";
        });
});