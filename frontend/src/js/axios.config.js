import Axios from 'axios';

const axios = Axios.create(
    {
        baseURL: 'http://localhost:4300',
        timeout: 40000,
        crossDomain: true
    }
)

export default axios;