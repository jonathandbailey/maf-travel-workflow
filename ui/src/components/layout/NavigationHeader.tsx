import { MenuFoldOutlined, MenuUnfoldOutlined } from "@ant-design/icons";
import { Button, Flex } from "antd";

interface NavigationHeaderProps {
    collapsed: boolean;
    setCollapsed: (collapsed: boolean) => void;
}

const NavigationHeader = ({ collapsed, setCollapsed }: NavigationHeaderProps) => {
    return (
        <>
            <Flex justify="end">
                <Button
                    type="text"
                    icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
                    onClick={() => setCollapsed(!collapsed)}
                    style={{
                        fontSize: '16px',
                        width: 64,
                        height: 64,
                    }}
                />
            </Flex>
        </>);
};

export default NavigationHeader;
