window.setImage = async function (byteArray, canvasElementId, width, height) {
    const canvas = document.getElementById(canvasElementId);
    const context = canvas.getContext("2d");

    const imageData = new ImageData(new Uint8ClampedArray(byteArray), width, height);
    const imageBitmap = await createImageBitmap(imageData);

    context.clearRect(0, 0, canvas.width, canvas.height);
    context.drawImage(imageBitmap, 0, 0, canvas.width, canvas.height);
    imageBitmap.close();
}