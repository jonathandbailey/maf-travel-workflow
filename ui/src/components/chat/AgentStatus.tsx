import { Timeline } from "antd";
import type { Status } from "../../types/ui/Status";

interface AgentStatusProps {
    statusItems: Status[];
}

const AgentStatus = ({ statusItems }: AgentStatusProps) => {
    return (
        <>
            <div >
                <Timeline>
                    {statusItems.map((status, index) => (
                        <Timeline.Item key={index} >
                            {status.message}
                        </Timeline.Item>
                    ))}
                </Timeline>
            </div>
        </>
    )
}

export default AgentStatus;