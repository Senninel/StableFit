window.getGeolocation = () => new Promise((resolve, reject) => {
    if (!navigator.geolocation) {
        reject('Geolocation is not supported by your browser.');
        return;
    }
    navigator.geolocation.getCurrentPosition(
        position => resolve({
            latitude: position.coords.latitude,
            longitude: position.coords.longitude
        }),
        error => reject(error.message),
        { timeout: 10000, enableHighAccuracy: false }
    );
});
