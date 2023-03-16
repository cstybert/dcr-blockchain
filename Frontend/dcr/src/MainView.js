import { Box, Button, Code, color, Container, FormControl, HStack, Input, Text, VStack } from '@chakra-ui/react';
import { useState, Fragment } from 'react';
import axios from 'axios';
import JSONViewer from 'react-json-viewer';


const example = {
    string: 'this is a test string',
    integer: 42,
    array: [1, 2, 3, 'test', NaN],
    float: 3.14159,
    undefined,
    object: {
      'first-child': true,
      'second-child': false,
      'last-child': null,
    },
    string_number: '1234',
    date: new Date(),
  };

export default function MainView() {
    const [GraphId, setGraphId] = useState(null);
    const [Graph, setGraph] = useState(example);

    const GetGraph = async  () => {
        const {data} = await axios.get("http://127.0.0.1:4300/DCR/" + GraphId);
        setGraph(data);
    }
    return (
    <Container>
        <HStack justifyContent={'space-evenly'} mt={30}>
        <Box>
                <Input  onChange={event => setGraphId(event.currentTarget.value)} placeholder='Enter id'/>
                <Button onClick={GetGraph} mt={3}>
                    <Text>Get</Text>
                </Button>
                <JSONViewer json={Graph}/>
        </Box>
        <Box>
            <Text>
                test2
            </Text>
            </Box>
        </HStack>
    </Container>
  );
}