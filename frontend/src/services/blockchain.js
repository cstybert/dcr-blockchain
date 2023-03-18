import axios from "../js/axios.config"

const apiUrl = "/DCR";

export default {
    async getGraph(id) {
        return axios.get(`${apiUrl}/${id}`).then(res => {
            return res.data;
        });
    }
}