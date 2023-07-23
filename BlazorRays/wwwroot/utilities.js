window.setImage = function (byteArray) {
    const canvas = document.getElementById("image");
    let context = canvas.getContext("2d");

    let imageData = new ImageData(new Uint8ClampedArray(byteArray), canvas.width, canvas.height);

    context.clearRect(0, 0, canvas.width, canvas.height);
    context.putImageData(imageData, 0, 0);
}