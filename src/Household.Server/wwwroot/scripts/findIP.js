function findIP() {
    return new Promise(function (resolve, reject) {
        try {
            var myPeerConnection = window.RTCPeerConnection || window.mozRTCPeerConnection || window.webkitRTCPeerConnection;
            var pc = new myPeerConnection({ iceServers: [] });
            var localIPs = {};
            var ipRegex = /([0-9]{1,3}(\.[0-9]{1,3}){3}|[a-f0-9]{1,4}(:[a-f0-9]{1,4}){7})/g;

            function ipIterate(ip) {
                if (!localIPs[ip]) {
                    console.log("Discovered IP: ", ip); // Debug output
                    localIPs[ip] = true;
                }
            }

            pc.createDataChannel("");
            pc.createOffer()
                .then(function (sdp) {
                    sdp.sdp.split('\n').forEach(function (line) {
                        if (line.indexOf('candidate') < 0) return;
                        line.match(ipRegex)?.forEach(ipIterate);
                    });
                    return pc.setLocalDescription(sdp);
                })
                .catch((err) => console.error("Error during createOffer: ", err));

            pc.onicecandidate = function (ice) {
                if (ice && ice.candidate && ice.candidate.candidate) {
                    ice.candidate.candidate.match(ipRegex)?.forEach(ipIterate);
                }

                if (!ice.candidate) {
                    // ICE candidate gathering complete
                    resolve(Object.keys(localIPs)); // Resolve with the list of IPs
                }
            };
        } catch (ex) {
            console.error("Error in findIP function: ", ex);
            reject(Error(ex));
        }
    });
}
