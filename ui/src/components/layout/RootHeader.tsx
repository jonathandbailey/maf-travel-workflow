import { Flex, Typography } from "antd";
import TravelIcon from '../../assets/fly.png';

const { Title } = Typography;

const RootHeader = () => {
    return (
        <>
            <Flex justify="start" align="center" style={{ height: "100%" }}>
                <img
                    src={TravelIcon}
                    alt="Travel App Logo"
                    style={{ height: '64px', width: 'auto', marginRight: '0px' }}
                />
                <Title level={4} style={{ marginLeft: "2px", marginBottom: 0, marginTop: 0 }}>Travel Planner</Title>
            </Flex>
        </>);
}
export default RootHeader;