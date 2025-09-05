export function createObjectURL(blobRef) {
    return URL.createObjectURL(blobRef);
}

export function downloadObjectUrl(url, fileName) {
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
}

// You must have this function too!
export async function getBlobData(blobRef) {
    const arrayBuffer = await blobRef.arrayBuffer();
    return new Uint8Array(arrayBuffer);
}

export function playAudio(audioId) {
    const audio = document.getElementById(audioId);
    if (audio) {
        audio.play();
    }
}