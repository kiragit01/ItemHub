document.addEventListener('DOMContentLoaded', function () {
    const thumbnails = document.querySelector('.thumbs-container');
    const thumbnailImages = document.querySelectorAll('.thumbnail');
    const mainImage = document.getElementById('main-image');
    const prevThumbButton = document.querySelector('.thumb-nav.prev');
    const nextThumbButton = document.querySelector('.thumb-nav.next');
    const prevMainButton = document.querySelector('.main-nav.prev');
    const nextMainButton = document.querySelector('.main-nav.next');

    let currentIndex = 0;

    function updateMainImage(src) {
        mainImage.src = src;
    }

    function updateActiveThumbnail() {
        thumbnailImages.forEach((thumbnail, index) => {
            if (index === currentIndex) {
                thumbnail.classList.add('thumbnail-active');
            } else {
                thumbnail.classList.remove('thumbnail-active');
            }
        });
    }

    function updateThumbnailPosition() {
        updateActiveThumbnail();
        updateMainImage(thumbnailImages[currentIndex].src);
        ensureThumbnailInView();
    }

    function ensureThumbnailInView() {
        const activeThumbnail = thumbnailImages[currentIndex];
        const thumbnailsContainer = thumbnails.parentElement;

        const thumbnailLeft = activeThumbnail.offsetLeft;
        const thumbnailRight = thumbnailLeft + activeThumbnail.offsetWidth;

        const containerLeft = thumbnailsContainer.scrollLeft;
        const containerRight = containerLeft + thumbnailsContainer.offsetWidth;

        if (thumbnailLeft < containerLeft) {
            thumbnailsContainer.scrollTo({ left: thumbnailLeft, behavior: 'smooth' });
        } else if (thumbnailRight > containerRight) {
            thumbnailsContainer.scrollTo({ left: thumbnailRight - thumbnailsContainer.offsetWidth, behavior: 'smooth' });
        }
    }

    prevThumbButton.addEventListener('click', function () {
        currentIndex--;
        if (currentIndex < 0) {
            currentIndex = thumbnailImages.length - 1;
        }
        updateThumbnailPosition();
    });

    nextThumbButton.addEventListener('click', function () {
        currentIndex++;
        if (currentIndex >= thumbnailImages.length) {
            currentIndex = 0;
        }
        updateThumbnailPosition();
    });

    prevMainButton.addEventListener('click', function () {
        currentIndex--;
        if (currentIndex < 0) {
            currentIndex = thumbnailImages.length - 1;
        }
        updateMainImage(thumbnailImages[currentIndex].src);
        updateThumbnailPosition();
    });

    nextMainButton.addEventListener('click', function () {
        currentIndex++;
        if (currentIndex >= thumbnailImages.length) {
            currentIndex = 0;
        }
        updateMainImage(thumbnailImages[currentIndex].src);
        updateThumbnailPosition();
    });

    thumbnailImages.forEach((thumbnail, index) => {
        thumbnail.addEventListener('click', function () {
            currentIndex = index;
            updateThumbnailPosition();
        });
    });

    updateThumbnailPosition(); // Начальное положение
});
