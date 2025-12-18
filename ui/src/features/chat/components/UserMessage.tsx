import { Card } from "antd";
import type { UIMessage } from "../../../types/ui/UIMessage";

interface UserMessageProps {
    message: UIMessage;
}

const UserMessage = ({ message }: UserMessageProps) => {
    return (
        <Card
            style={{
                marginLeft: "auto",
                marginBottom: 0,
                marginRight: 0,
                textAlign: "left",
                backgroundColor: "#f5f5f5",
                padding: 0,
            }}
            styles={{ body: { padding: "4px 16px" } }}
        >
            {message.text}
        </Card>
    );
};

export default UserMessage;